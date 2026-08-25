using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace BookStore.Services
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _configuration;

        public VnPayService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreatePaymentUrl(VnPayPaymentRequest request)
        {
            string vnp_Url = _configuration["VnPay:Url"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
            string vnp_TmnCode = _configuration["VnPay:TmnCode"] ?? "0P3L0W46";
            string vnp_HashSecret = _configuration["VnPay:HashSecret"] ?? "G4XG1K8U7G7N84Y0T5K3T5I2G1V4N8Y4";

            string ipAddress = string.IsNullOrWhiteSpace(request.IpAddress) || request.IpAddress == "::1" || request.IpAddress == "0.0.0.1" 
                ? "127.0.0.1" 
                : request.IpAddress;

            var vnp_Params = new SortedDictionary<string, string>(new VnPayCompare())
            {
                { "vnp_Version", "2.1.0" },
                { "vnp_Command", "pay" },
                { "vnp_TmnCode", vnp_TmnCode.Trim() },
                { "vnp_Amount", ((long)(request.Amount * 100)).ToString() },
                { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") },
                { "vnp_CurrCode", "VND" },
                { "vnp_IpAddr", ipAddress },
                { "vnp_Locale", "vn" },
                { "vnp_OrderInfo", request.OrderInfo },
                { "vnp_OrderType", "other" },
                { "vnp_ReturnUrl", request.ReturnUrl },
                { "vnp_TxnRef", request.OrderId }
            };

            var data = new StringBuilder();
            foreach (var kv in vnp_Params)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                }
            }

            string queryString = data.ToString();
            string signData = queryString.Length > 0 ? queryString.Remove(queryString.Length - 1, 1) : "";
            string vnp_SecureHash = HmacSha512(vnp_HashSecret.Trim(), signData);

            return $"{vnp_Url}?{queryString}vnp_SecureHash={vnp_SecureHash}";
        }

        public VnPayPaymentResponse ProcessPaymentCallback(IQueryCollection query)
        {
            string vnp_HashSecret = _configuration["VnPay:HashSecret"] ?? "G4XG1K8U7G7N84Y0T5K3T5I2G1V4N8Y4";

            var vnp_Params = new SortedDictionary<string, string>(new VnPayCompare());
            string vnp_SecureHash = string.Empty;

            foreach (var key in query.Keys)
            {
                if (key.Equals("vnp_SecureHash", StringComparison.OrdinalIgnoreCase))
                {
                    vnp_SecureHash = query[key].ToString();
                }
                else if (key.StartsWith("vnp_", StringComparison.OrdinalIgnoreCase))
                {
                    vnp_Params.Add(key, query[key].ToString());
                }
            }

            var data = new StringBuilder();
            foreach (var kv in vnp_Params)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                }
            }

            string queryString = data.ToString();
            string signData = queryString.Length > 0 ? queryString.Remove(queryString.Length - 1, 1) : "";
            string checkSignature = HmacSha512(vnp_HashSecret.Trim(), signData);

            bool isValidSignature = checkSignature.Equals(vnp_SecureHash, StringComparison.OrdinalIgnoreCase);
            string responseCode = query["vnp_ResponseCode"].ToString();
            string orderId = query["vnp_TxnRef"].ToString();
            string transactionId = query["vnp_TransactionNo"].ToString();

            bool isSuccess = isValidSignature && responseCode == "00";

            return new VnPayPaymentResponse
            {
                Success = isSuccess,
                OrderId = orderId,
                TransactionId = transactionId,
                ResponseCode = responseCode,
                Message = isSuccess ? "Giao dịch thành công qua VNPay" : "Giao dịch không thành công hoặc chữ ký không hợp lệ"
            };
        }

        private static string HmacSha512(string key, string inputData)
        {
            var hash = new StringBuilder();
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }
            return hash.ToString();
        }
    }

    public class VnPayCompare : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            var vnpCompare = System.Globalization.CompareInfo.GetCompareInfo("en-US");
            return vnpCompare.Compare(x, y, System.Globalization.CompareOptions.Ordinal);
        }
    }
}

namespace BookStore.Services
{
    public class VnPayPaymentRequest
    {
        public string OrderId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string OrderInfo { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;
        public string IpAddress { get; set; } = "127.0.0.1";
    }

    public class VnPayPaymentResponse
    {
        public bool Success { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string ResponseCode { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public interface IVnPayService
    {
        string CreatePaymentUrl(VnPayPaymentRequest request);
        VnPayPaymentResponse ProcessPaymentCallback(IQueryCollection query);
    }
}

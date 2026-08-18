using BookStore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Pages.Checkout
{
    public class VnPayReturnModel : PageModel
    {
        private readonly IVnPayService _vnPayService;
        private readonly IOrderService _orderService;
        private readonly ILogger<VnPayReturnModel> _logger;

        public VnPayReturnModel(
            IVnPayService vnPayService,
            IOrderService orderService,
            ILogger<VnPayReturnModel> logger)
        {
            _vnPayService = vnPayService;
            _orderService = orderService;
            _logger = logger;
        }

        public bool IsSuccess { get; set; }
        public string OrderInfo { get; set; } = string.Empty;
        public string TransactionNo { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int CreatedOrderId { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            var response = _vnPayService.ProcessPaymentCallback(Request.Query);

            IsSuccess = response.Success;
            OrderInfo = response.OrderId;
            TransactionNo = response.TransactionId;

            if (decimal.TryParse(Request.Query["vnp_Amount"], out var rawAmount))
            {
                Amount = rawAmount / 100m;
            }

            if (IsSuccess)
            {
                string? userId = HttpContext.Session.GetString("VnPay_UserId");
                string? bookIdsCsv = HttpContext.Session.GetString("VnPay_BookIds");
                int? addressId = HttpContext.Session.GetInt32("VnPay_AddressId");
                string? voucherCode = HttpContext.Session.GetString("VnPay_VoucherCode");

                if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(bookIdsCsv) && addressId.HasValue)
                {
                    var bookIds = bookIdsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                            .Select(s => int.TryParse(s, out var i) ? i : 0)
                                            .Where(i => i > 0)
                                            .ToList();

                    var createReq = new CreateOrderRequest
                    {
                        UserId = userId,
                        SelectedBookIds = bookIds,
                        AddressId = addressId.Value,
                        PaymentMethod = "VNPAY",
                        VoucherCode = voucherCode
                    };

                    var (success, msg, orderId) = await _orderService.CreateOrderAsync(createReq);
                    if (success)
                    {
                        CreatedOrderId = orderId;
                        _logger.LogInformation("VNPay order #{OrderId} placed successfully for user {UserId}", orderId, userId);
                    }
                    else
                    {
                        _logger.LogError("Failed to create order for VNPay callback: {Msg}", msg);
                    }
                }

                // Clean session keys
                HttpContext.Session.Remove("VnPay_UserId");
                HttpContext.Session.Remove("VnPay_BookIds");
                HttpContext.Session.Remove("VnPay_AddressId");
                HttpContext.Session.Remove("VnPay_VoucherCode");
                HttpContext.Session.Remove("VnPay_TxnRef");
            }
            else
            {
                ErrorMessage = response.Message;
            }

            return Page();
        }
    }
}

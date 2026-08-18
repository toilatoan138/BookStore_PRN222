using BookStore.Models.Entities;
using BookStore.Models.Enums;

namespace BookStore.Services
{
    public class CheckoutCalculationResult
    {
        public decimal SubTotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal VoucherDiscount { get; set; }
        public decimal FPointDiscount { get; set; }
        public decimal TotalDiscount => VoucherDiscount + FPointDiscount;
        public decimal FinalTotal { get; set; }
        public int? AppliedVoucherId { get; set; }
        public string? VoucherMessage { get; set; }
    }

    public class CreateOrderRequest
    {
        public string UserId { get; set; } = string.Empty;
        public List<int> SelectedBookIds { get; set; } = new();
        public int AddressId { get; set; }
        public string PaymentMethod { get; set; } = "COD"; // COD, VNPAY, WALLET
        public string? VoucherCode { get; set; }
        public decimal ShippingFee { get; set; } = 30000;
    }

    public interface IOrderService
    {
        Task<CheckoutCalculationResult> CalculateCheckoutAsync(string userId, List<int> selectedBookIds, string? voucherCode, string paymentMethod, decimal shippingFee = 30000);
        Task<(bool Success, string Message, int OrderId)> CreateOrderAsync(CreateOrderRequest request);
        Task<Order?> GetOrderByIdAsync(int orderId, string? userId = null);
        Task<List<Order>> GetUserOrdersAsync(string userId, OrderStatus? status = null);
        Task<bool> CancelOrderAsync(int orderId, string userId, string reason);
        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, string? note = null);
    }
}

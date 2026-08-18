using BookStore.Models.Entities;
using BookStore.Models.Enums;

namespace BookStore.Services
{
    public class AdminDashboardStats
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalBooksSold { get; set; }
        public int TotalCustomers { get; set; }
        public int PendingOrdersCount { get; set; }
        public int LowStockBooksCount { get; set; }
        public int PendingReturnsCount { get; set; }
        public int PendingPOCount { get; set; }
        public List<Order> RecentOrders { get; set; } = new();
        public List<Book> TopSellingBooks { get; set; } = new();
    }

    public class UserManagementItem
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public bool Status { get; set; }
        public decimal TotalSpend { get; set; }
        public int FPoints { get; set; }
        public decimal WalletBalance { get; set; }
        public string Role { get; set; } = "Customer";
        public DateTime CreatedAt { get; set; }
    }

    public interface IAdminService
    {
        Task<AdminDashboardStats> GetDashboardStatsAsync();
        Task<List<UserManagementItem>> GetAllUsersAsync(string? keyword = null);
        Task<bool> ToggleUserStatusAsync(string userId);
        Task<bool> SetUserRoleAsync(string userId, string newRole);
        Task<List<Order>> GetAllOrdersAsync(OrderStatus? status = null, string? keyword = null);
        Task<List<PurchaseOrder>> GetPurchaseOrdersAsync(int? status = null);
        Task<bool> ApprovePurchaseOrderAsync(int poId, string adminUserId, string? note = null);
        Task<bool> CancelPurchaseOrderAsync(int poId, string adminUserId, string reason);
        Task<List<ReturnRequest>> GetReturnRequestsAsync(int? status = null);
        Task<bool> ReviewReturnRequestAsync(int returnId, int status, string? adminNote, decimal? refundAmount = null);
        Task<List<Voucher>> GetAllVouchersAsync();
        Task<Voucher> CreateVoucherAsync(Voucher voucher);
        Task<bool> ToggleVoucherStatusAsync(int voucherId);
    }
}

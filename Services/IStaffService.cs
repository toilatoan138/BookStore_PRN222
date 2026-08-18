using BookStore.Models.Entities;

namespace BookStore.Services
{
    public class StaffDashboardStats
    {
        public int TotalCustomers { get; set; }
        public int PendingTicketsCount { get; set; }
        public int TotalReviewsCount { get; set; }
        public int ActivePromotionsCount { get; set; }
        public List<SupportTicket> RecentTickets { get; set; } = new();
        public List<Review> RecentReviews { get; set; } = new();
    }

    public interface IStaffService
    {
        Task<StaffDashboardStats> GetDashboardStatsAsync();
        Task<List<ApplicationUser>> GetCustomersAsync(string? keyword = null);
        Task<ApplicationUser?> GetCustomerDetailAsync(string userId);
        Task<List<CustomerNote>> GetCustomerNotesAsync(string userId);
        Task<CustomerNote> AddCustomerNoteAsync(string userId, string? contactChannel, string noteContent, DateTime? followUpDate);
        Task<List<Review>> GetReviewsAsync(int? rating = null, bool? hasReply = null);
        Task<bool> ReplyToReviewAsync(int reviewId, string staffReply);
        Task<List<SupportTicket>> GetTicketsAsync(string? status = null);
        Task<bool> ReplyToTicketAsync(int ticketId, string adminReply, string status);
        Task<List<Promotion>> GetPromotionsAsync();
        Task<Promotion> CreatePromotionAsync(Promotion promo, List<int> bookIds);
        Task<bool> TogglePromotionStatusAsync(int promoId);
        Task<List<FPointHistory>> GetAllFPointHistoriesAsync();
    }
}

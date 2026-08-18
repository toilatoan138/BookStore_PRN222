using BookStore.Models.Entities;

namespace BookStore.Services
{
    public interface ISupportTicketService
    {
        Task<List<SupportTicket>> GetUserTicketsAsync(string userId);
        Task<SupportTicket?> GetTicketByIdAsync(int ticketId, string? userId = null);
        Task<SupportTicket> CreateTicketAsync(string userId, string issueType, string subject, string message);
    }
}

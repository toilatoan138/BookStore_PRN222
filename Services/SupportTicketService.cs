using BookStore.Data;
using BookStore.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Services
{
    public class SupportTicketService : ISupportTicketService
    {
        private readonly ApplicationDbContext _context;

        public SupportTicketService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<SupportTicket>> GetUserTicketsAsync(string userId)
        {
            return await _context.SupportTickets
                .Where(st => st.UserId == userId)
                .OrderByDescending(st => st.CreatedAt)
                .ToListAsync();
        }

        public async Task<SupportTicket?> GetTicketByIdAsync(int ticketId, string? userId = null)
        {
            var query = _context.SupportTickets.AsQueryable();
            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(st => st.TicketId == ticketId && st.UserId == userId);
            }
            else
            {
                query = query.Where(st => st.TicketId == ticketId);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<SupportTicket> CreateTicketAsync(string userId, string issueType, string subject, string message)
        {
            var ticket = new SupportTicket
            {
                UserId = userId,
                IssueType = issueType,
                TicketSubject = subject,
                TicketMessage = message,
                Status = "Open",
                CreatedAt = DateTime.UtcNow
            };

            _context.SupportTickets.Add(ticket);
            await _context.SaveChangesAsync();
            return ticket;
        }
    }
}

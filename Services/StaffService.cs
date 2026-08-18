using BookStore.Data;
using BookStore.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Services
{
    public class StaffService : IStaffService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StaffService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<StaffDashboardStats> GetDashboardStatsAsync()
        {
            var stats = new StaffDashboardStats
            {
                TotalCustomers = await _userManager.Users.CountAsync(),
                PendingTicketsCount = await _context.SupportTickets.CountAsync(t => t.Status != "Closed"),
                TotalReviewsCount = await _context.Reviews.CountAsync(),
                ActivePromotionsCount = await _context.Promotions.CountAsync(p => p.IsActive && p.EndDate >= DateTime.UtcNow),
                RecentTickets = await _context.SupportTickets
                    .Include(t => t.User)
                    .OrderByDescending(t => t.CreatedAt)
                    .Take(5)
                    .ToListAsync(),
                RecentReviews = await _context.Reviews
                    .Include(r => r.Book)
                    .Include(r => r.User)
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(5)
                    .ToListAsync()
            };

            return stats;
        }

        public async Task<List<ApplicationUser>> GetCustomersAsync(string? keyword = null)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string kw = keyword.Trim().ToLower();
                query = query.Where(u => (u.FullName != null && u.FullName.ToLower().Contains(kw)) ||
                                         (u.Email != null && u.Email.ToLower().Contains(kw)) ||
                                         (u.PhoneNumber != null && u.PhoneNumber.Contains(kw)));
            }

            return await query.OrderByDescending(u => u.TotalSpend).ToListAsync();
        }

        public async Task<ApplicationUser?> GetCustomerDetailAsync(string userId)
        {
            return await _userManager.Users
                .Include(u => u.Addresses)
                .Include(u => u.Orders)
                .Include(u => u.CustomerNotes)
                .Include(u => u.FPointHistories)
                .Include(u => u.WalletHistories)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<List<CustomerNote>> GetCustomerNotesAsync(string userId)
        {
            return await _context.CustomerNotes
                .Where(cn => cn.UserId == userId)
                .OrderByDescending(cn => cn.CreatedAt)
                .ToListAsync();
        }

        public async Task<CustomerNote> AddCustomerNoteAsync(string userId, string? contactChannel, string noteContent, DateTime? followUpDate)
        {
            var note = new CustomerNote
            {
                UserId = userId,
                ContactChannel = contactChannel ?? "Trực tiếp",
                NoteContent = noteContent,
                FollowUpDate = followUpDate,
                CreatedAt = DateTime.UtcNow
            };

            _context.CustomerNotes.Add(note);
            await _context.SaveChangesAsync();
            return note;
        }

        public async Task<List<Review>> GetReviewsAsync(int? rating = null, bool? hasReply = null)
        {
            var query = _context.Reviews
                .Include(r => r.Book)
                .Include(r => r.User)
                .AsQueryable();

            if (rating.HasValue && rating.Value > 0)
            {
                query = query.Where(r => r.Rating == rating.Value);
            }

            if (hasReply.HasValue)
            {
                if (hasReply.Value)
                {
                    query = query.Where(r => !string.IsNullOrEmpty(r.StaffReply));
                }
                else
                {
                    query = query.Where(r => string.IsNullOrEmpty(r.StaffReply));
                }
            }

            return await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
        }

        public async Task<bool> ReplyToReviewAsync(int reviewId, string staffReply)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null) return false;

            review.StaffReply = staffReply;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<SupportTicket>> GetTicketsAsync(string? status = null)
        {
            var query = _context.SupportTickets
                .Include(t => t.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(t => t.Status == status);
            }

            return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
        }

        public async Task<bool> ReplyToTicketAsync(int ticketId, string adminReply, string status)
        {
            var ticket = await _context.SupportTickets.FindAsync(ticketId);
            if (ticket == null) return false;

            ticket.AdminReply = adminReply;
            ticket.Status = status;

            // Notify user
            _context.Notifications.Add(new Notification
            {
                UserId = ticket.UserId,
                Message = $"Yêu cầu hỗ trợ #{ticket.TicketId} của bạn đã có phản hồi mới từ nhân viên MindBook.",
                Link = "/Support/Index",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Promotion>> GetPromotionsAsync()
        {
            return await _context.Promotions
                .Include(p => p.PromotionBooks)
                    .ThenInclude(pb => pb.Book)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();
        }

        public async Task<Promotion> CreatePromotionAsync(Promotion promo, List<int> bookIds)
        {
            _context.Promotions.Add(promo);
            await _context.SaveChangesAsync();

            if (bookIds != null && bookIds.Any())
            {
                foreach (var bid in bookIds)
                {
                    _context.PromotionBooks.Add(new PromotionBook
                    {
                        PromoId = promo.PromoId,
                        BookId = bid
                    });
                }
                await _context.SaveChangesAsync();
            }

            return promo;
        }

        public async Task<bool> TogglePromotionStatusAsync(int promoId)
        {
            var promo = await _context.Promotions.FindAsync(promoId);
            if (promo == null) return false;

            promo.IsActive = !promo.IsActive;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<FPointHistory>> GetAllFPointHistoriesAsync()
        {
            return await _context.FPointHistories
                .Include(f => f.User)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }
    }
}

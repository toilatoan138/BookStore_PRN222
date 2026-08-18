using BookStore.Data;
using BookStore.Models.Entities;
using BookStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Pages.Staff.Reviews
{
    [Authorize(Roles = "Staff,Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IStaffService _staffService;

        public IndexModel(ApplicationDbContext context, IStaffService staffService)
        {
            _context = context;
            _staffService = staffService;
        }

        public List<Review> Reviews { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? Rating { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Keyword { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool? HasReply { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.Reviews
                .Include(r => r.Book)
                .Include(r => r.User)
                .AsNoTracking()
                .AsQueryable();

            if (Rating.HasValue && Rating.Value > 0)
            {
                query = query.Where(r => r.Rating == Rating.Value);
            }

            if (HasReply.HasValue)
            {
                if (HasReply.Value)
                    query = query.Where(r => !string.IsNullOrEmpty(r.StaffReply));
                else
                    query = query.Where(r => string.IsNullOrEmpty(r.StaffReply));
            }

            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                string kw = Keyword.Trim().ToLower();
                query = query.Where(r => (r.Book != null && r.Book.Title != null && r.Book.Title.ToLower().Contains(kw)) ||
                                         (r.Comment != null && r.Comment.ToLower().Contains(kw)) ||
                                         (r.User != null && r.User.FullName != null && r.User.FullName.ToLower().Contains(kw)));
            }

            Reviews = await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostReplyAsync(int reviewId, string staffReply)
        {
            if (string.IsNullOrWhiteSpace(staffReply))
            {
                TempData["ErrorMessage"] = "Nội dung phản hồi không được để trống.";
                return RedirectToPage(new { rating = Rating, keyword = Keyword, hasReply = HasReply });
            }

            var review = await _context.Reviews.FindAsync(reviewId);
            if (review != null)
            {
                review.StaffReply = staffReply.Trim();
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã gửi phản hồi đánh giá công khai cho khách hàng thành công!";
            }

            return RedirectToPage(new { rating = Rating, keyword = Keyword, hasReply = HasReply });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int reviewId)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review != null)
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa đánh giá vi phạm thành công!";
            }
            return RedirectToPage(new { rating = Rating, keyword = Keyword, hasReply = HasReply });
        }
    }
}

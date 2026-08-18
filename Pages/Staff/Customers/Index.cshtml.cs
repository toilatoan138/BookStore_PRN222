using BookStore.Data;
using BookStore.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Pages.Staff.Customers
{
    [Authorize(Roles = "Staff,Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<ApplicationUser> Customers { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Keyword { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? MemberTier { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? MinPoint { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? MaxPoint { get; set; }

        public async Task OnGetAsync()
        {
            var query = _userManager.Users
                .Include(u => u.CustomerNotes)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                string kw = Keyword.Trim().ToLower();
                query = query.Where(u => (u.FullName != null && u.FullName.ToLower().Contains(kw)) ||
                                         (u.Email != null && u.Email.ToLower().Contains(kw)) ||
                                         (u.PhoneNumber != null && u.PhoneNumber.Contains(kw)) ||
                                         (u.Id != null && u.Id.Contains(kw)));
            }

            if (!string.IsNullOrWhiteSpace(MemberTier) && MemberTier != "all")
            {
                switch (MemberTier.ToLower())
                {
                    case "diamond":
                        query = query.Where(u => u.FPoints >= 5000);
                        break;
                    case "gold":
                        query = query.Where(u => u.FPoints >= 2000 && u.FPoints < 5000);
                        break;
                    case "silver":
                        query = query.Where(u => u.FPoints >= 500 && u.FPoints < 2000);
                        break;
                    case "bronze":
                        query = query.Where(u => u.FPoints < 500);
                        break;
                }
            }

            if (MinPoint.HasValue)
            {
                query = query.Where(u => u.FPoints >= MinPoint.Value);
            }

            if (MaxPoint.HasValue)
            {
                query = query.Where(u => u.FPoints <= MaxPoint.Value);
            }

            Customers = await query
                .OrderByDescending(u => u.TotalSpend)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostApplyTagAsync([FromForm] List<string> selectedUserIds, [FromForm] string tag)
        {
            if (selectedUserIds != null && selectedUserIds.Any() && !string.IsNullOrWhiteSpace(tag))
            {
                var users = await _context.Users.Where(u => selectedUserIds.Contains(u.Id)).ToListAsync();
                foreach (var user in users)
                {
                    var existingTags = string.IsNullOrEmpty(user.Tags) ? new List<string>() : user.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
                    if (!existingTags.Contains(tag.Trim()))
                    {
                        existingTags.Add(tag.Trim());
                        user.Tags = string.Join(", ", existingTags);
                    }
                }
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã gắn nhãn '{tag}' cho {users.Count} khách hàng được chọn!";
            }
            return RedirectToPage(new { keyword = Keyword, memberTier = MemberTier, minPoint = MinPoint, maxPoint = MaxPoint });
        }

        public async Task<IActionResult> OnPostAddNoteAsync([FromForm] List<string> selectedUserIds, [FromForm] string note)
        {
            if (selectedUserIds != null && selectedUserIds.Any() && !string.IsNullOrWhiteSpace(note))
            {
                foreach (var userId in selectedUserIds)
                {
                    _context.CustomerNotes.Add(new CustomerNote
                    {
                        UserId = userId,
                        ContactChannel = "CSKH",
                        NoteContent = note.Trim(),
                        CreatedAt = DateTime.UtcNow
                    });
                }
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã thêm ghi chú nội bộ cho {selectedUserIds.Count} khách hàng!";
            }
            return RedirectToPage(new { keyword = Keyword, memberTier = MemberTier, minPoint = MinPoint, maxPoint = MaxPoint });
        }

        public async Task<IActionResult> OnPostSendMarketingAsync([FromForm] List<string> selectedUserIds, [FromForm] string subject, [FromForm] string content)
        {
            if (selectedUserIds != null && selectedUserIds.Any() && !string.IsNullOrWhiteSpace(subject))
            {
                string msg = $"{subject.Trim()}: {content?.Trim()}";
                foreach (var userId in selectedUserIds)
                {
                    _context.Notifications.Add(new Notification
                    {
                        UserId = userId,
                        Message = msg.Length > 500 ? msg.Substring(0, 500) : msg,
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã gửi thông báo ưu đãi / marketing thành công tới {selectedUserIds.Count} khách hàng!";
            }
            return RedirectToPage(new { keyword = Keyword, memberTier = MemberTier, minPoint = MinPoint, maxPoint = MaxPoint });
        }
    }
}

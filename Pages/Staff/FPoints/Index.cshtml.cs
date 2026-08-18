using BookStore.Data;
using BookStore.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Pages.Staff.FPoints
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

        public List<FPointHistory> Histories { get; set; } = new();
        public List<ApplicationUser> AvailableCustomers { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Keyword { get; set; }

        public async Task OnGetAsync()
        {
            AvailableCustomers = await _userManager.Users
                .OrderBy(u => u.FullName)
                .Take(50)
                .ToListAsync();

            var query = _context.FPointHistories
                .Include(h => h.User)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                string kw = Keyword.Trim().ToLower();
                query = query.Where(h => (h.CustomerInfo != null && h.CustomerInfo.ToLower().Contains(kw)) ||
                                         (h.Reason != null && h.Reason.ToLower().Contains(kw)) ||
                                         (h.User != null && h.User.FullName.ToLower().Contains(kw)) ||
                                         (h.User != null && h.User.Email != null && h.User.Email.ToLower().Contains(kw)));
            }

            Histories = await query
                .OrderByDescending(h => h.CreatedAt)
                .Take(100)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostExecutePointAsync(string userIdentifier, string actionType, int amount, string reason)
        {
            if (string.IsNullOrWhiteSpace(userIdentifier) || amount <= 0 || string.IsNullOrWhiteSpace(reason))
            {
                TempData["ErrorMessage"] = "Vui lòng điền đầy đủ thông tin khách hàng, số điểm và lý do.";
                return RedirectToPage(new { keyword = Keyword });
            }

            string identifier = userIdentifier.Trim();
            var targetUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == identifier || u.Email == identifier || u.PhoneNumber == identifier || u.UserName == identifier);

            if (targetUser == null)
            {
                TempData["ErrorMessage"] = $"Không tìm thấy khách hàng với thông tin '{userIdentifier}'.";
                return RedirectToPage(new { keyword = Keyword });
            }

            if (actionType == "sub" && targetUser.FPoints < amount)
            {
                TempData["ErrorMessage"] = $"Khách hàng chỉ có {targetUser.FPoints} điểm, không thể trừ {amount} điểm!";
                return RedirectToPage(new { keyword = Keyword });
            }

            if (actionType == "add")
            {
                targetUser.FPoints += amount;
            }
            else
            {
                targetUser.FPoints -= amount;
            }

            _context.FPointHistories.Add(new FPointHistory
            {
                UserId = targetUser.Id,
                CustomerInfo = $"{targetUser.FullName} ({targetUser.Email})",
                ActionType = actionType,
                Amount = amount,
                Reason = reason.Trim(),
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã {(actionType == "add" ? "cộng" : "trừ")} {amount} F-Point cho khách hàng '{targetUser.FullName}' thành công!";

            return RedirectToPage(new { keyword = Keyword });
        }
    }
}

using BookStore.Data;
using BookStore.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Pages.Admin.Users
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ViewModel phụ để hiển thị User kèm Role trực quan ra bảng
        public class UserViewModel : ApplicationUser
        {
            public string Role { get; set; } = "Customer";
        }

        public List<UserViewModel> Users { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Keyword { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.Users.AsNoTracking().AsQueryable();

            // Tìm kiếm thông minh theo Tên, Email hoặc Username
            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                string kw = Keyword.Trim().ToLower();
                query = query.Where(u =>
                    (u.FullName != null && u.FullName.ToLower().Contains(kw)) ||
                    (u.Email != null && u.Email.ToLower().Contains(kw)) ||
                    (u.UserName != null && u.UserName.ToLower().Contains(kw)));
            }

            var baseUsers = await query.OrderByDescending(u => u.CreatedAt).ToListAsync();

            Users = new List<UserViewModel>();
            foreach (var user in baseUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                Users.Add(new UserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    FullName = user.FullName,
                    Status = user.Status,
                    CreatedAt = user.CreatedAt,
                    TotalSpend = user.TotalSpend,
                    FPoints = user.FPoints,
                    WalletBalance = user.WalletBalance,
                    Role = roles.FirstOrDefault() ?? "Customer"
                });
            }
        }

        // 1. Xử lý đổi trạng thái Khóa / Mở khóa tài khoản
        public async Task<IActionResult> OnPostToggleStatusAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng!";
                return RedirectToPage();
            }

            // Đảo ngược trạng thái Active <-> Banned
            user.Status = !user.Status;
            await _userManager.UpdateAsync(user);

            TempData["SuccessMessage"] = $"Đã cập nhật trạng thái tài khoản '{user.FullName}' thành công!";
            return RedirectToPage();
        }

        // 2. Xử lý đổi Vai trò (Role) trực tiếp từ dropdown
        public async Task<IActionResult> OnPostChangeRoleAsync(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return RedirectToPage();

            var currentRoles = await _userManager.GetRolesAsync(user);

            // Xóa hết role cũ và gán role mới
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, newRole);

            TempData["SuccessMessage"] = $"Đã chuyển vai trò của '{user.FullName}' sang {newRole}!";
            return RedirectToPage();
        }
    }
}
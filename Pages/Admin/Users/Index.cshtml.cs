using BookStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Pages.Admin.Users
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IAdminService _adminService;

        public IndexModel(IAdminService adminService)
        {
            _adminService = adminService;
        }

        public List<UserManagementItem> Users { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Keyword { get; set; }

        public async Task OnGetAsync()
        {
            Users = await _adminService.GetAllUsersAsync(Keyword);
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(string userId)
        {
            bool success = await _adminService.ToggleUserStatusAsync(userId);
            if (success)
            {
                TempData["SuccessMessage"] = "Đã cập nhật trạng thái hoạt động của người dùng!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
            }

            return RedirectToPage(new { keyword = Keyword });
        }

        public async Task<IActionResult> OnPostChangeRoleAsync(string userId, string newRole)
        {
            bool success = await _adminService.SetUserRoleAsync(userId, newRole);
            if (success)
            {
                TempData["SuccessMessage"] = $"Đã cập nhật vai trò thành '{newRole}'!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể thay đổi vai trò.";
            }

            return RedirectToPage(new { keyword = Keyword });
        }
    }
}

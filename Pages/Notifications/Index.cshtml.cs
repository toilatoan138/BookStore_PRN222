using BookStore.Models.Entities;
using BookStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Pages.Notifications
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly INotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(INotificationService notificationService, UserManager<ApplicationUser> userManager)
        {
            _notificationService = notificationService;
            _userManager = userManager;
        }

        public List<Notification> Notifications { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            Notifications = await _notificationService.GetUserNotificationsAsync(user.Id, 30);
            return Page();
        }

        public async Task<IActionResult> OnPostMarkAllReadAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            await _notificationService.MarkAllAsReadAsync(user.Id);
            TempData["SuccessMessage"] = "Đã đánh dấu tất cả thông báo là đã đọc!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostMarkReadAsync(int id, string? link)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            await _notificationService.MarkAsReadAsync(id, user.Id);
            if (!string.IsNullOrEmpty(link) && Url.IsLocalUrl(link))
            {
                return LocalRedirect(link);
            }
            return RedirectToPage();
        }
    }
}

using BookStore.Models.Entities;
using BookStore.Models.Enums;
using BookStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Pages.Orders
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(IOrderService orderService, UserManager<ApplicationUser> userManager)
        {
            _orderService = orderService;
            _userManager = userManager;
        }

        public List<Order> Orders { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public OrderStatus? Status { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            Orders = await _orderService.GetUserOrdersAsync(user.Id, Status);
            return Page();
        }

        public async Task<IActionResult> OnPostCancelAsync(int orderId, string reason)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            bool success = await _orderService.CancelOrderAsync(orderId, user.Id, reason ?? "Khách hàng yêu cầu hủy");
            if (success)
            {
                TempData["SuccessMessage"] = $"Đã hủy đơn hàng #{orderId} thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể hủy đơn hàng này (đơn đã được giao vận hoặc xử lý).";
            }

            return RedirectToPage(new { status = Status });
        }
    }
}

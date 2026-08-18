using BookStore.Models.Entities;
using BookStore.Models.Enums;
using BookStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Pages.Admin.Orders
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IAdminService _adminService;
        private readonly IOrderService _orderService;

        public IndexModel(IAdminService adminService, IOrderService orderService)
        {
            _adminService = adminService;
            _orderService = orderService;
        }

        public List<Order> Orders { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public OrderStatus? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Keyword { get; set; }

        public async Task OnGetAsync()
        {
            Orders = await _adminService.GetAllOrdersAsync(Status, Keyword);
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int orderId, OrderStatus newStatus)
        {
            await _orderService.UpdateOrderStatusAsync(orderId, newStatus);
            TempData["SuccessMessage"] = $"Đã cập nhật trạng thái đơn #{orderId} thành '{newStatus}'!";
            return RedirectToPage(new { status = Status, keyword = Keyword });
        }
    }
}

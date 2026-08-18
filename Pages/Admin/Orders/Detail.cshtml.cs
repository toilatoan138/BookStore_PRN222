using BookStore.Models.Entities;
using BookStore.Models.Enums;
using BookStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Pages.Admin.Orders
{
    [Authorize(Roles = "Admin")]
    public class DetailModel : PageModel
    {
        private readonly IOrderService _orderService;

        public DetailModel(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public Order Order { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null) return NotFound();

            Order = order;
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int id, OrderStatus newStatus, string? note)
        {
            await _orderService.UpdateOrderStatusAsync(id, newStatus, note);
            TempData["SuccessMessage"] = $"Đã cập nhật trạng thái đơn #{id} thành '{newStatus}'!";
            return RedirectToPage(new { id });
        }
    }
}

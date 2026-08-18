using BookStore.Data;
using BookStore.Models.Entities;
using BookStore.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Pages.Warehouse.Orders
{
    [Authorize(Roles = "Warehouse,Admin")]
    public class PickingModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public PickingModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Order? OrderInfo { get; set; }

        [BindProperty(SupportsGet = true)]
        public int OrderId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            OrderInfo = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Details)
                    .ThenInclude(d => d.Book)
                .FirstOrDefaultAsync(o => o.Id == OrderId);

            if (OrderInfo == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng cần lấy hàng!";
                return RedirectToPage("/Warehouse/Orders/Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostCompletePickingAsync(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.Status = OrderStatus.Packed;
                order.StatusNote = "Đã hoàn thành soạn hàng (Picking) tại kho";
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã hoàn thành soạn hàng cho Đơn #{orderId}. Đơn sẵn sàng đóng gói & xuất kho!";
            }

            return RedirectToPage("/Warehouse/Orders/Index");
        }
    }
}

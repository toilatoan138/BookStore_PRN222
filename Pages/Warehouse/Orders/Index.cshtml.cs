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
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Order> Orders { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public int Status { get; set; } = 0;

        public async Task OnGetAsync()
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.Details)
                    .ThenInclude(d => d.Book)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(Search))
            {
                string kw = Search.Trim().ToLower();
                query = query.Where(o => (o.FullName != null && o.FullName.ToLower().Contains(kw)) ||
                                         (o.PhoneNumber != null && o.PhoneNumber.Contains(kw)) ||
                                         (o.User != null && o.User.FullName != null && o.User.FullName.ToLower().Contains(kw)));
            }

            if (Status > 0)
            {
                query = query.Where(o => (int)o.Status == Status);
            }

            Orders = await query
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostShipOrderAsync(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null && order.Status == OrderStatus.Packed)
            {
                order.Status = OrderStatus.Shipping;
                order.StatusNote = "Đã bàn giao đơn hàng cho đơn vị vận chuyển";
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đơn hàng #{orderId} đã được xuất kho và chuyển sang trạng thái Đang giao!";
            }
            return RedirectToPage(new { search = Search, status = Status });
        }
    }
}

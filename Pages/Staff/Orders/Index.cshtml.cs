using BookStore.Data;
using BookStore.Models.Entities;
using BookStore.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Pages.Staff.Orders
{
    [Authorize(Roles = "Staff,Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Order> Orders { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Keyword { get; set; }

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

            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                string kw = Keyword.Trim().ToLower();
                query = query.Where(o => (o.FullName != null && o.FullName.ToLower().Contains(kw)) ||
                                         (o.PhoneNumber != null && o.PhoneNumber.Contains(kw)) ||
                                         (o.User != null && o.User.FullName != null && o.User.FullName.ToLower().Contains(kw)) ||
                                         o.Id.ToString() == kw);
            }

            if (Status > 0)
            {
                query = query.Where(o => (int)o.Status == Status);
            }

            Orders = await query
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostApproveAsync(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null && order.Status == OrderStatus.Pending)
            {
                order.Status = OrderStatus.Processing;
                order.StatusNote = "Nhân viên đã xác nhận đơn hàng, chuyển tiếp xuống kho soạn hàng";
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã duyệt đơn hàng #{orderId} và chuyển xuống kho để soạn hàng!";
            }
            return RedirectToPage(new { keyword = Keyword, status = Status });
        }

        public async Task<IActionResult> OnPostCancelAsync(int orderId, string? cancelReason)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null && (order.Status == OrderStatus.Pending || order.Status == OrderStatus.Processing))
            {
                order.Status = OrderStatus.Cancelled;
                order.StatusNote = cancelReason?.Trim() ?? "Nhân viên đã hủy đơn hàng";
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã hủy đơn hàng #{orderId}!";
            }
            return RedirectToPage(new { keyword = Keyword, status = Status });
        }
    }
}

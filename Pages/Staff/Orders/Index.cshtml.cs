using BookStore.Data;
using BookStore.Models.Entities;
using BookStore.Models.Enums;
using BookStore.Services;
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
        private readonly IOrderService _orderService;

        public IndexModel(ApplicationDbContext context, IOrderService orderService)
        {
            _context = context;
            _orderService = orderService;
        }

        public List<Order> Orders { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Keyword { get; set; }

        [BindProperty(SupportsGet = true)]
        public int Status { get; set; } = 0;

        // THÊM BIẾN PHÂN TRANG Ở ĐÂY
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 10; // Đúng 10 đơn/trang theo RDS

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

            // TÍNH TOÁN PHÂN TRANG
            int totalCount = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
            if (CurrentPage < 1) CurrentPage = 1;
            if (CurrentPage > TotalPages && TotalPages > 0) CurrentPage = TotalPages;

            // SỬA LẠI ĐOẠN LẤY DỮ LIỆU BẰNG SKIP VÀ TAKE
            Orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
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
            return RedirectToPage(new { Keyword, Status, CurrentPage });
        }

        public async Task<IActionResult> OnPostCancelAsync(int orderId, string? cancelReason)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null && (order.Status == OrderStatus.Pending || order.Status == OrderStatus.Processing))
            {
                bool success = await _orderService.CancelOrderAsync(orderId, order.UserId ?? string.Empty, cancelReason?.Trim() ?? "Nhân viên đã hủy đơn hàng");
                if (success)
                {
                    TempData["SuccessMessage"] = $"Đã hủy đơn hàng #{orderId} và tự động hoàn tồn kho, tiền ví & thu hồi điểm thưởng tương ứng!";
                }
                else
                {
                    TempData["ErrorMessage"] = $"Không thể hủy đơn hàng #{orderId}.";
                }
            }
            return RedirectToPage(new { Keyword, Status, CurrentPage });
        }
    }
}
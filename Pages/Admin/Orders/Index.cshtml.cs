using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookStore.Data;
using BookStore.Models.Entities;
using BookStore.Models.Enums;
using BookStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Pages.Admin.Orders
{
    [Authorize(Roles = "Admin,Staff")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOrderService _orderService;

        public IndexModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IOrderService orderService)
        {
            _context = context;
            _userManager = userManager;
            _orderService = orderService;
        }

        public List<Order> Orders { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public OrderStatus? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Keyword { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            var roles = await _userManager.GetRolesAsync(user);
            bool isSuperAdmin = roles.Contains("Admin") && (!user.BranchId.HasValue || user.BranchId == 0);

            var query = _context.Orders
                .Include(o => o.Details)
                .ThenInclude(d => d.Book)
                .AsNoTracking()
                .AsQueryable();

            // TEST 3 (Staff Isolation): Nhân viên chi nhánh chỉ xem đơn của chi nhánh mình
            if (!isSuperAdmin && user.BranchId.HasValue && user.BranchId.Value > 0)
            {
                query = query.Where(o => o.BranchId == user.BranchId.Value);
            }

            // Lọc theo Tab trạng thái
            if (Status.HasValue)
            {
                query = query.Where(o => o.Status == Status.Value);
            }

            // Tìm kiếm thông minh
            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                string kw = Keyword.Trim().ToLower();
                query = query.Where(o =>
                    o.Id.ToString().Contains(kw) ||
                    (o.FullName != null && o.FullName.ToLower().Contains(kw)) ||
                    (o.PhoneNumber != null && o.PhoneNumber.Contains(kw)));
            }

            Orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int orderId, OrderStatus newStatus)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng!";
                return RedirectToPage(new { status = Status, keyword = Keyword });
            }

            // TEST 3: Kiểm tra quyền xử lý đơn thuộc chi nhánh
            var roles = await _userManager.GetRolesAsync(user);
            bool isSuperAdmin = roles.Contains("Admin") && (!user.BranchId.HasValue || user.BranchId == 0);
            if (!isSuperAdmin && user.BranchId.HasValue && order.BranchId.HasValue && order.BranchId != user.BranchId)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền can thiệp đơn hàng thuộc chi nhánh khác!";
                return RedirectToPage(new { status = Status, keyword = Keyword });
            }

            // TEST 1 & TEST 2: Kiểm tra ma trận chuyển trạng thái hợp lệ
            if (!IsValidTransition(order.Status, newStatus, out string errorMessage))
            {
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToPage(new { status = Status, keyword = Keyword });
            }

            await _orderService.UpdateOrderStatusAsync(orderId, newStatus);
            TempData["SuccessMessage"] = $"Đã cập nhật trạng thái đơn #{orderId} thành '{newStatus}'!";
            return RedirectToPage(new { status = Status, keyword = Keyword });
        }

        public static bool IsValidTransition(OrderStatus current, OrderStatus target, out string error)
        {
            error = string.Empty;
            if (current == target) return true;

            // TEST 2: Chặn hủy đơn khi đã giao thành công
            if (current == OrderStatus.Delivered)
            {
                if (target == OrderStatus.Cancelled)
                {
                    error = "Đơn hàng đã giao thành công không thể hủy, vui lòng chuyển qua luồng Yêu cầu Trả hàng.";
                }
                else
                {
                    error = $"Đơn hàng đã giao thành công không thể đổi ngược về trạng thái '{target}'.";
                }
                return false;
            }

            // TEST 1: Chặn đổi trạng thái đơn đã hủy
            if (current == OrderStatus.Cancelled)
            {
                error = "Đơn hàng đã bị hủy, không thể thay đổi trạng thái!";
                return false;
            }

            if ((int)current >= 5 || current.ToString().StartsWith("Return"))
            {
                error = "Đơn hàng đang trong quy trình trả hàng, vui lòng xử lý tại mục Yêu cầu Trả hàng!";
                return false;
            }

            switch (current)
            {
                case OrderStatus.Pending:
                    if (target == OrderStatus.Processing || target == OrderStatus.Cancelled) return true;
                    error = "Đơn hàng chờ duyệt chỉ có thể chuyển sang 'Đang chuẩn bị' hoặc 'Đã hủy'!";
                    return false;

                case OrderStatus.Processing:
                    if (target == OrderStatus.Shipping || target == OrderStatus.Cancelled) return true;
                    error = "Đơn hàng đang chuẩn bị chỉ có thể chuyển sang 'Đang giao' hoặc 'Đã hủy'!";
                    return false;

                case OrderStatus.Shipping:
                    if (target == OrderStatus.Delivered || target == OrderStatus.Cancelled) return true;
                    error = "Đơn hàng đang giao chỉ có thể chuyển sang 'Đã giao' hoặc 'Đã hủy'!";
                    return false;

                default:
                    return true;
            }
        }
    }
}
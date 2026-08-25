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
    public class DetailModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOrderService _orderService;

        public DetailModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IOrderService orderService)
        {
            _context = context;
            _userManager = userManager;
            _orderService = orderService;
        }

        public Order Order { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            var order = await _context.Orders
                .Include(o => o.Details)
                .ThenInclude(d => d.Book)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            // TEST 3 (Staff Isolation): Ngăn nhân viên chi nhánh truy cập trực tiếp đơn chi nhánh khác
            var roles = await _userManager.GetRolesAsync(user);
            bool isSuperAdmin = roles.Contains("Admin") && (!user.BranchId.HasValue || user.BranchId == 0);
            if (!isSuperAdmin && user.BranchId.HasValue && order.BranchId.HasValue && order.BranchId != user.BranchId)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập đơn hàng thuộc chi nhánh khác!";
                return RedirectToPage("/Admin/Orders/Index");
            }

            Order = order;
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int id, OrderStatus newStatus, string? note)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();

            // TEST 3: Kiểm tra quyền chi nhánh
            var roles = await _userManager.GetRolesAsync(user);
            bool isSuperAdmin = roles.Contains("Admin") && (!user.BranchId.HasValue || user.BranchId == 0);
            if (!isSuperAdmin && user.BranchId.HasValue && order.BranchId.HasValue && order.BranchId != user.BranchId)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền cập nhật đơn hàng thuộc chi nhánh khác!";
                return RedirectToPage("/Admin/Orders/Index");
            }

            // TEST 1 & TEST 2: Kiểm tra hợp lệ luồng trạng thái
            if (!IndexModel.IsValidTransition(order.Status, newStatus, out string errorMessage))
            {
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToPage(new { id });
            }

            string? safeNote = string.IsNullOrWhiteSpace(note) ? null : (note.Trim().Length > 500 ? note.Trim().Substring(0, 500) : note.Trim());
            await _orderService.UpdateOrderStatusAsync(id, newStatus, safeNote);

            TempData["SuccessMessage"] = $"Đã cập nhật trạng thái đơn #{id} thành '{newStatus}'!";
            return RedirectToPage(new { id });
        }
    }
}
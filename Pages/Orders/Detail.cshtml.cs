using System.ComponentModel.DataAnnotations;
using BookStore.Data;
using BookStore.Models.Entities;
using BookStore.Models.Enums;
using BookStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Pages.Orders
{
    [Authorize]
    public class DetailModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public DetailModel(
            IOrderService orderService,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _orderService = orderService;
            _userManager = userManager;
            _context = context;
        }

        public Order Order { get; set; } = null!;
        public List<ReturnRequest> ExistingReturns { get; set; } = new();

        [BindProperty]
        public ReturnFormModel ReturnInput { get; set; } = new();

        public class ReturnFormModel
        {
            [Required]
            public int BookId { get; set; }

            [Required]
            [Range(1, int.MaxValue)]
            public int Quantity { get; set; } = 1;

            [Required(ErrorMessage = "Vui lòng nhập lý do trả hàng")]
            [StringLength(500, ErrorMessage = "Lý do không được vượt quá 500 ký tự")]
            public string CustomerReason { get; set; } = string.Empty;

            [StringLength(100)]
            public string? BankName { get; set; }

            [StringLength(50)]
            public string? AccountNumber { get; set; }

            [StringLength(100)]
            public string? AccountOwner { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            var order = await _orderService.GetOrderByIdAsync(id, user.Id);
            if (order == null)
            {
                return NotFound();
            }

            Order = order;
            ExistingReturns = await _context.ReturnRequests
                .Include(r => r.Book)
                .Where(r => r.OrderId == id)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostCancelAsync(int id, string reason)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            string safeReason = string.IsNullOrWhiteSpace(reason) ? "Khách hàng yêu cầu hủy" : (reason.Length > 500 ? reason.Substring(0, 500) : reason);
            bool success = await _orderService.CancelOrderAsync(id, user.Id, safeReason);
            if (success)
            {
                TempData["SuccessMessage"] = "Đã hủy đơn hàng thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể hủy đơn hàng này.";
            }

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostConfirmReceivedAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            var order = await _orderService.GetOrderByIdAsync(id, user.Id);
            if (order == null || order.Status != OrderStatus.Shipping)
            {
                TempData["ErrorMessage"] = "Không thể cập nhật trạng thái đơn hàng này.";
                return RedirectToPage(new { id });
            }

            bool success = await _orderService.UpdateOrderStatusAsync(id, OrderStatus.Delivered, "Khách hàng xác nhận đã nhận hàng thành công");
            if (success)
            {
                TempData["SuccessMessage"] = "Cảm ơn bạn đã xác nhận nhận hàng! Đơn hàng đã hoàn tất.";
            }
            else
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi cập nhật trạng thái.";
            }

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostReturnRequestAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            var order = await _orderService.GetOrderByIdAsync(id, user.Id);
            if (order == null || order.Status != OrderStatus.Delivered)
            {
                TempData["ErrorMessage"] = "Chỉ có thể yêu cầu trả hàng cho đơn hàng đã giao thành công.";
                return RedirectToPage(new { id });
            }

            var item = order.Details.FirstOrDefault(d => d.BookId == ReturnInput.BookId);
            if (item == null)
            {
                TempData["ErrorMessage"] = "Sản phẩm không thuộc đơn hàng này.";
                return RedirectToPage(new { id });
            }

            // Cắt chuỗi lý do tối đa 500 ký tự chống tràn dữ liệu Database
            string safeReason = (ReturnInput.CustomerReason ?? string.Empty).Trim();
            if (safeReason.Length > 500)
            {
                safeReason = safeReason.Substring(0, 500);
            }

            int validQty = Math.Clamp(ReturnInput.Quantity, 1, item.Quantity);

            var returnReq = new ReturnRequest
            {
                OrderId = id,
                BookId = ReturnInput.BookId,
                Quantity = validQty,
                CustomerReason = safeReason,
                Price = item.Price,
                MaxRefundableAmount = order.TotalAmount, // Hạn mức hoàn tiền lấy bằng tổng giá trị toàn bộ đơn hàng
                BankName = ReturnInput.BankName?.Trim(),
                AccountNumber = ReturnInput.AccountNumber?.Trim(),
                AccountOwner = ReturnInput.AccountOwner?.Trim(),
                Status = 0, // Pending
                CreatedAt = DateTime.UtcNow
            };

            order.Status = OrderStatus.ReturnRequested;
            _context.ReturnRequests.Add(returnReq);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Yêu cầu trả hàng đã được gửi thành công! MindBook sẽ xem xét và phản hồi sớm.";
            return RedirectToPage(new { id });
        }
    }
}
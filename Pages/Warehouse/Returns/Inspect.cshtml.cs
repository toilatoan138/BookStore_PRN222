using BookStore.Data;
using BookStore.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Pages.Warehouse.Returns
{
    [Authorize(Roles = "Warehouse,Admin")]
    public class InspectModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public InspectModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public ReturnRequest? ReturnItem { get; set; }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            ReturnItem = await _context.ReturnRequests
                .Include(r => r.Order)
                    .ThenInclude(o => o.User)
                .Include(r => r.Book)
                .FirstOrDefaultAsync(r => r.ReturnId == Id);

            if (ReturnItem == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy yêu cầu trả hàng cần kiểm tra!";
                return RedirectToPage("/Warehouse/Returns/Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostPassQCAsync(int returnId, string? qcNote)
        {
            var user = await _userManager.GetUserAsync(User);
            var returnReq = await _context.ReturnRequests
                .Include(r => r.Book)
                .Include(r => r.Order)
                .FirstOrDefaultAsync(r => r.ReturnId == returnId);

            if (returnReq != null)
            {
                returnReq.Status = 3; // Completed / Restocked
                returnReq.AdminNote = (returnReq.AdminNote + " | Kho: Đạt kiểm định QC và đã nhập lại kho.").TrimStart('|', ' ');

                // Hoàn nhập số lượng tồn kho cho sách
                if (returnReq.Book != null)
                {
                    returnReq.Book.StockQuantity += returnReq.Quantity;

                    // Ghi lịch sử biến động kho
                    _context.InventoryHistories.Add(new InventoryHistory
                    {
                        BookId = returnReq.BookId,
                        CreatedById = user?.Id,
                        TransactionType = "RETURN",
                        QuantityChanged = returnReq.Quantity,
                        RelatedId = returnReq.OrderId,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                // Nếu đơn hàng có trạng thái return request, cập nhật trạng thái đơn
                if (returnReq.Order != null)
                {
                    returnReq.Order.Status = BookStore.Models.Enums.OrderStatus.ReturnCompleted;
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã hoàn thành kiểm định QC cho đơn trả #{returnId} và cộng +{returnReq.Quantity} quyển vào tồn kho!";
            }

            return RedirectToPage("/Warehouse/Returns/Index");
        }

        public async Task<IActionResult> OnPostRejectQCAsync(int returnId, string? qcNote)
        {
            var returnReq = await _context.ReturnRequests
                .Include(r => r.Order)
                .FirstOrDefaultAsync(r => r.ReturnId == returnId);

            if (returnReq != null)
            {
                returnReq.Status = 2; // Rejected
                returnReq.AdminNote = (returnReq.AdminNote + $" | Kho: Không đạt QC ({qcNote ?? "Hàng hư hỏng/không đủ điều kiện"}).").TrimStart('|', ' ');

                await _context.SaveChangesAsync();
                TempData["ErrorMessage"] = $"Đã từ chối nhập kho cho yêu cầu trả hàng #{returnId} do không đạt chuẩn QC!";
            }

            return RedirectToPage("/Warehouse/Returns/Index");
        }
    }
}

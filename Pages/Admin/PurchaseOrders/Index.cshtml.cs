using BookStore.Data;
using BookStore.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Pages.Admin.PurchaseOrders
{
    [Authorize(Roles = "Admin,Staff")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<PurchaseOrder> PurchaseOrders { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? Status { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.CreatedBy)
                .AsNoTracking()
                .AsQueryable();

            // Lọc theo trạng thái nếu có chọn tab tương ứng
            if (Status.HasValue)
            {
                query = query.Where(po => po.Status == Status.Value);
            }

            PurchaseOrders = await query
                .OrderByDescending(po => po.OrderDate)
                .ToListAsync();
        }

        // 1. Phê duyệt đơn nhập hàng (Status = 1: Đã duyệt)
        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            var po = await _context.PurchaseOrders.FindAsync(id);
            if (po == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn nhập hàng!";
                return RedirectToPage();
            }

            if (po.Status != 0)
            {
                TempData["ErrorMessage"] = "Đơn hàng này không ở trạng thái chờ duyệt.";
                return RedirectToPage();
            }

            po.Status = 1; // Chuyển sang Đã duyệt
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã phê duyệt đơn nhập hàng #{po.PurchaseOrderId} thành công!";
            return RedirectToPage(new { status = Status });
        }

        // 2. Từ chối đơn nhập hàng (Status = 3: Đã hủy)
        public async Task<IActionResult> OnPostCancelAsync(int id, string reason)
        {
            var po = await _context.PurchaseOrders.FindAsync(id);
            if (po == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn nhập hàng!";
                return RedirectToPage();
            }

            po.Status = 3; // Đã từ chối / Hủy
            // Nếu bảng PO có cột lưu lý do từ chối, bạn có thể gán vào đây (ví dụ: po.Note = reason;)
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã từ chối đơn nhập hàng #{po.PurchaseOrderId}.";
            return RedirectToPage(new { status = Status });
        }
    }
}
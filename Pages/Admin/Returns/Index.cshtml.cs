using BookStore.Data;
using BookStore.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Pages.Admin.Returns
{
    [Authorize(Roles = "Admin,Staff")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<ReturnRequest> Returns { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? Status { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.ReturnRequests
                .Include(r => r.Order)
                .Include(r => r.Book)
                .AsNoTracking()
                .AsQueryable();

            if (Status.HasValue)
            {
                query = query.Where(r => r.Status == Status.Value);
            }

            Returns = await query
                .OrderByDescending(r => r.ReturnId)
                .ToListAsync();
        }

        // Xử lý quyết định duyệt/từ chối/hoàn tiền yêu cầu trả hàng từ Modal
        // Xử lý quyết định duyệt/từ chối/hoàn tiền yêu cầu trả hàng từ Modal
        public async Task<IActionResult> OnPostReviewAsync(int id, int newStatus, decimal? refundAmount, string note)
        {
            var returnReq = await _context.ReturnRequests
                .Include(r => r.Order)
                .FirstOrDefaultAsync(r => r.ReturnId == id);

            if (returnReq == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy yêu cầu trả hàng!";
                return RedirectToPage();
            }

            // Cập nhật trạng thái yêu cầu
            returnReq.Status = newStatus;

            // Nếu Admin/Staff chọn hoàn tiền vào ví (newStatus == 3) và có phát sinh số tiền
            if (newStatus == 3 && refundAmount.HasValue && refundAmount.Value > 0)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == returnReq.Order.UserId);
                if (user != null)
                {
                    // Cộng tiền trực tiếp vào số dư ví của khách
                    user.WalletBalance += refundAmount.Value;
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã xử lý thành công yêu cầu trả hàng #{returnReq.ReturnId}!";
            return RedirectToPage(new { status = Status });
        }
    }
}
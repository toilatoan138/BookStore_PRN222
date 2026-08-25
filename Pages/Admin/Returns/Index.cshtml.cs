using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public int TotalCount { get; set; }
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RefundedCount { get; set; }
        public int RejectedCount { get; set; }

        public async Task OnGetAsync()
        {
            TotalCount = await _context.ReturnRequests.CountAsync();
            PendingCount = await _context.ReturnRequests.CountAsync(r => r.Status == 0);
            ApprovedCount = await _context.ReturnRequests.CountAsync(r => r.Status == 1);
            RefundedCount = await _context.ReturnRequests.CountAsync(r => r.Status == 3);
            RejectedCount = await _context.ReturnRequests.CountAsync(r => r.Status == 2);

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

            // Đồng bộ hạn mức hoàn tiền tối đa theo tổng đơn hàng
            foreach (var r in Returns)
            {
                if (r.Order != null && r.Order.TotalAmount > 0)
                {
                    r.MaxRefundableAmount = r.Order.TotalAmount;
                }
            }
        }

        public async Task<IActionResult> OnPostReviewAsync(int id, int newStatus, decimal? refundAmount, string? note)
        {
            var returnReq = await _context.ReturnRequests
                .Include(r => r.Order)
                .Include(r => r.Book)
                .FirstOrDefaultAsync(r => r.ReturnId == id);

            if (returnReq == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy yêu cầu trả hàng!";
                return RedirectToPage(new { status = Status });
            }

            if (returnReq.Status == 3 || returnReq.Status == 2)
            {
                TempData["ErrorMessage"] = "Yêu cầu trả hàng này đã được xử lý hoàn tất trước đó!";
                return RedirectToPage(new { status = Status });
            }

            if (newStatus == 3) // Hoàn tiền ngay vào ví
            {
                // Hạn mức tối đa lấy theo tổng tiền đơn hàng
                decimal allowedMax = (returnReq.Order != null && returnReq.Order.TotalAmount > 0)
                    ? returnReq.Order.TotalAmount
                    : (returnReq.MaxRefundableAmount > 0 ? returnReq.MaxRefundableAmount : 0);

                // TEST 2 (Backend Validation): Chặn số âm hoặc 0
                if (!refundAmount.HasValue || refundAmount.Value <= 0)
                {
                    TempData["ErrorMessage"] = "Số tiền hoàn vào ví phải lớn hơn 0 VNĐ.";
                    return RedirectToPage(new { status = Status });
                }

                // TEST 1 (Backend Validation): Chặn vượt quá tổng giá trị đơn hàng
                if (allowedMax > 0 && refundAmount.Value > allowedMax)
                {
                    TempData["ErrorMessage"] = $"Số tiền hoàn ({refundAmount.Value:N0}đ) không được vượt quá tổng giá trị đơn hàng ({allowedMax:N0}đ).";
                    return RedirectToPage(new { status = Status });
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == returnReq.Order.UserId);
                if (user != null)
                {
                    user.WalletBalance += refundAmount.Value;
                }

                returnReq.Status = 3; // Đã hoàn tiền
                TempData["SuccessMessage"] = $"Đã duyệt và hoàn {refundAmount.Value:N0}đ vào Ví của khách hàng thành công!";
            }
            else if (newStatus == 1)
            {
                returnReq.Status = 1;
                TempData["SuccessMessage"] = $"Đã chấp thuận yêu cầu trả hàng #{returnReq.ReturnId}. Chờ nhận sách về kho!";
            }
            else if (newStatus == 2)
            {
                returnReq.Status = 2;
                TempData["SuccessMessage"] = $"Đã từ chối yêu cầu trả hàng #{returnReq.ReturnId}.";
            }

            await _context.SaveChangesAsync();
            return RedirectToPage(new { status = Status });
        }
    }
}
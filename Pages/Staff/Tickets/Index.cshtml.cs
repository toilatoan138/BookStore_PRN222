using BookStore.Data;
using BookStore.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Pages.Staff.Tickets
{
    [Authorize(Roles = "Staff,Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<SupportTicket> Tickets { get; set; } = new();
        public List<ReturnRequest> ReturnRequests { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Tab { get; set; } = "ticket";

        public async Task OnGetAsync()
        {
            Tickets = await _context.SupportTickets
                .Include(t => t.User)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            ReturnRequests = await _context.ReturnRequests
                .Include(r => r.Order)
                    .ThenInclude(o => o.User)
                .Include(r => r.Book)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostReplyTicketAsync(int ticketId, string adminReply, string newStatus)
        {
            // VÁ LỖI LOGIC: Bắt buộc phải có nội dung nếu đóng/phản hồi ticket
            if (string.IsNullOrWhiteSpace(adminReply))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập nội dung phản hồi ticket.";
                return RedirectToPage(new { tab = "ticket" });
            }

            // VÁ BẢO MẬT: Chống lỗi tràn Database (Giả sử DB tối đa 500 ký tự)
            if (adminReply.Length > 500)
            {
                TempData["ErrorMessage"] = "Nội dung phản hồi không được vượt quá 500 ký tự.";
                return RedirectToPage(new { tab = "ticket" });
            }

            var ticket = await _context.SupportTickets.FindAsync(ticketId);
            if (ticket != null)
            {
                ticket.AdminReply = adminReply.Trim();
                // Bảo vệ thao tác đổi trạng thái bậy bạ
                ticket.Status = (newStatus == "Closed") ? "Closed" : "Replied";

                if (!string.IsNullOrEmpty(ticket.UserId))
                {
                    _context.Notifications.Add(new Notification
                    {
                        UserId = ticket.UserId,
                        Message = $"Yêu cầu hỗ trợ #{ticket.TicketId} của bạn đã có phản hồi mới từ nhân viên MindBook.",
                        Link = "/Support/Index",
                        CreatedAt = DateTime.UtcNow
                    });
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã gửi phản hồi cho Ticket #TK-{ticketId:D5}!";
            }

            return RedirectToPage(new { tab = "ticket" });
        }

        public async Task<IActionResult> OnPostApproveReturnAsync(int returnId, string? note)
        {
            var ret = await _context.ReturnRequests.FindAsync(returnId);
            if (ret != null)
            {
                // VÁ LỖI LOGIC NGHIỆP VỤ: Chống lừa đảo, duyệt trùng lặp đơn đổi trả
                // Kiểm tra xem cuốn sách trong đơn hàng này đã từng được duyệt (Status 1) hoặc Nhập kho (Status 3) ở một yêu cầu khác chưa
                bool isDuplicateApproved = await _context.ReturnRequests.AnyAsync(r =>
                    r.OrderId == ret.OrderId
                    && r.BookId == ret.BookId
                    && r.ReturnId != returnId
                    && (r.Status == 1 || r.Status == 3));

                if (isDuplicateApproved)
                {
                    TempData["ErrorMessage"] = "CẢNH BÁO: Cuốn sách này thuộc đơn hàng này đã được duyệt hoàn trả trong một yêu cầu khác!";
                    return RedirectToPage(new { tab = "return" });
                }

                ret.Status = 1; // Approved - Awaiting Item & QC
                ret.AdminNote = note?.Trim() ?? "Đã duyệt yêu cầu. Khách hàng vui lòng gửi lại sách để kho kiểm hàng.";
                ret.ApprovedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã duyệt đơn trả hàng #RET-{returnId}. Đơn đã chuyển sang kho để kiểm định QC!";
            }
            return RedirectToPage(new { tab = "return" });
        }

        public async Task<IActionResult> OnPostRejectReturnAsync(int returnId, string? note)
        {
            var ret = await _context.ReturnRequests.FindAsync(returnId);
            if (ret != null)
            {
                ret.Status = 2; // Rejected
                ret.AdminNote = note?.Trim() ?? "Từ chối yêu cầu hoàn trả do không đáp ứng chính sách đổi trả.";
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã từ chối yêu cầu trả hàng #RET-{returnId}!";
            }
            return RedirectToPage(new { tab = "return" });
        }
    }
}
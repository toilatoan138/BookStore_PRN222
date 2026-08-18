using System.ComponentModel.DataAnnotations;
using BookStore.Models.Entities;
using BookStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Pages.Support
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ISupportTicketService _ticketService;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(ISupportTicketService ticketService, UserManager<ApplicationUser> userManager)
        {
            _ticketService = ticketService;
            _userManager = userManager;
        }

        public List<SupportTicket> Tickets { get; set; } = new();

        [BindProperty]
        public TicketInputModel Input { get; set; } = new();

        public class TicketInputModel
        {
            [Required(ErrorMessage = "Vui lòng chọn loại vấn đề")]
            [Display(Name = "Loại vấn đề")]
            public string IssueType { get; set; } = "Đơn hàng & Giao vận";

            [Required(ErrorMessage = "Vui lòng nhập tiêu đề yêu cầu")]
            [StringLength(200, MinimumLength = 5, ErrorMessage = "Tiêu đề từ 5 đến 200 ký tự")]
            [Display(Name = "Tiêu đề")]
            public string Subject { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập nội dung chi tiết")]
            [StringLength(2000, MinimumLength = 10, ErrorMessage = "Nội dung tối thiểu 10 ký tự")]
            [Display(Name = "Nội dung")]
            public string Message { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            Tickets = await _ticketService.GetUserTicketsAsync(user.Id);
            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            if (!ModelState.IsValid)
            {
                Tickets = await _ticketService.GetUserTicketsAsync(user.Id);
                return Page();
            }

            await _ticketService.CreateTicketAsync(
                user.Id,
                Input.IssueType,
                Input.Subject,
                Input.Message
            );

            TempData["SuccessMessage"] = "Gửi yêu cầu hỗ trợ thành công! Đội ngũ chăm sóc khách hàng sẽ phản hồi bạn sớm nhất.";
            return RedirectToPage();
        }
    }
}

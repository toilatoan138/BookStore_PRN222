using BookStore.Models.Entities;
using BookStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Pages.Admin.Returns
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IAdminService _adminService;

        public IndexModel(IAdminService adminService)
        {
            _adminService = adminService;
        }

        public List<ReturnRequest> Returns { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? Status { get; set; }

        public async Task OnGetAsync()
        {
            Returns = await _adminService.GetReturnRequestsAsync(Status);
        }

        public async Task<IActionResult> OnPostReviewAsync(int id, int newStatus, string? note, decimal? refundAmount)
        {
            bool success = await _adminService.ReviewReturnRequestAsync(id, newStatus, note, refundAmount);
            if (success)
            {
                TempData["SuccessMessage"] = "Đã cập nhật xử lý yêu cầu trả hàng & hoàn tiền!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy yêu cầu trả hàng.";
            }

            return RedirectToPage(new { status = Status });
        }
    }
}

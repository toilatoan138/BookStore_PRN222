using BookStore.Models.Entities;
using BookStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Pages.Admin.PurchaseOrders
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IAdminService _adminService;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(IAdminService adminService, UserManager<ApplicationUser> userManager)
        {
            _adminService = adminService;
            _userManager = userManager;
        }

        public List<PurchaseOrder> PurchaseOrders { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? Status { get; set; }

        public async Task OnGetAsync()
        {
            PurchaseOrders = await _adminService.GetPurchaseOrdersAsync(Status);
        }

        public async Task<IActionResult> OnPostApproveAsync(int id, string? note)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            bool success = await _adminService.ApprovePurchaseOrderAsync(id, user.Id, note);
            if (success)
            {
                TempData["SuccessMessage"] = $"Đã duyệt Đơn nhập hàng #{id} thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể duyệt đơn này.";
            }

            return RedirectToPage(new { status = Status });
        }

        public async Task<IActionResult> OnPostCancelAsync(int id, string reason)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            bool success = await _adminService.CancelPurchaseOrderAsync(id, user.Id, reason ?? "Admin từ chối");
            if (success)
            {
                TempData["SuccessMessage"] = $"Đã từ chối Đơn nhập hàng #{id}!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể từ chối đơn này.";
            }

            return RedirectToPage(new { status = Status });
        }
    }
}

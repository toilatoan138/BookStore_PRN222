using BookStore.Models.Entities;
using BookStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Pages.Admin.Vouchers
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IAdminService _adminService;

        public IndexModel(IAdminService adminService)
        {
            _adminService = adminService;
        }

        public List<Voucher> Vouchers { get; set; } = new();

        public async Task OnGetAsync()
        {
            Vouchers = await _adminService.GetAllVouchersAsync();
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(int id)
        {
            await _adminService.ToggleVoucherStatusAsync(id);
            TempData["SuccessMessage"] = "Đã thay đổi trạng thái phát hành voucher!";
            return RedirectToPage();
        }
    }
}

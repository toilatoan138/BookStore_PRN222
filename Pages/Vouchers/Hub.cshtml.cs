using BookStore.Models.Entities;
using BookStore.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Pages.Vouchers
{
    public class HubModel : PageModel
    {
        private readonly IVoucherService _voucherService;
        private readonly UserManager<ApplicationUser> _userManager;

        public HubModel(IVoucherService voucherService, UserManager<ApplicationUser> userManager)
        {
            _voucherService = voucherService;
            _userManager = userManager;
        }

        public List<Voucher> Vouchers { get; set; } = new();
        public HashSet<int> SavedVoucherIds { get; set; } = new();

        public async Task OnGetAsync()
        {
            Vouchers = await _voucherService.GetActiveVouchersAsync();

            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var userVouchers = await _voucherService.GetUserWalletVouchersAsync(user.Id);
                    SavedVoucherIds = userVouchers.Select(uv => uv.VoucherId).ToHashSet();
                }
            }
        }

        public async Task<IActionResult> OnPostSaveAsync(int voucherId)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToPage("/Account/Login", new { returnUrl = "/Vouchers/Hub" });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            var (success, message) = await _voucherService.SaveVoucherToWalletAsync(user.Id, voucherId);
            if (success)
            {
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }

            return RedirectToPage();
        }
    }
}

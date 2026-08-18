using BookStore.Models.Entities;
using BookStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Pages.Vouchers
{
    [Authorize]
    public class WalletModel : PageModel
    {
        private readonly IVoucherService _voucherService;
        private readonly UserManager<ApplicationUser> _userManager;

        public WalletModel(IVoucherService voucherService, UserManager<ApplicationUser> userManager)
        {
            _voucherService = voucherService;
            _userManager = userManager;
        }

        public List<UserVoucher> UserVouchers { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            UserVouchers = await _voucherService.GetUserWalletVouchersAsync(user.Id);
            return Page();
        }
    }
}

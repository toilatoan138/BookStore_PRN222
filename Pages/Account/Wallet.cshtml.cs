using BookStore.Models.Entities;
using BookStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Pages.Account
{
    [Authorize]
    public class WalletModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserService _userService;

        public WalletModel(UserManager<ApplicationUser> userManager, IUserService userService)
        {
            _userManager = userManager;
            _userService = userService;
        }

        public ApplicationUser CurrentUser { get; set; } = null!;
        public List<WalletHistory> Histories { get; set; } = new();

        [BindProperty]
        public decimal DepositAmount { get; set; } = 100000;

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            CurrentUser = user;
            Histories = await _userService.GetWalletHistoriesAsync(user.Id);
            return Page();
        }

        public async Task<IActionResult> OnPostDepositAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            if (DepositAmount < 10000)
            {
                TempData["ErrorMessage"] = "Số tiền nạp tối thiểu là 10.000đ.";
                return RedirectToPage();
            }

            await _userService.UpdateWalletBalanceAsync(
                user.Id,
                DepositAmount,
                "TOPUP",
                $"Nạp tiền vào Ví BookStore (+{DepositAmount:N0}đ)"
            );

            TempData["SuccessMessage"] = $"Nạp thành công {DepositAmount:N0}đ vào ví!";
            return RedirectToPage();
        }
    }
}

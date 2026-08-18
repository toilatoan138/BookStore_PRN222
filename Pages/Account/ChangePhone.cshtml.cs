using System.ComponentModel.DataAnnotations;
using BookStore.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Pages.Account
{
    [Authorize]
    public class ChangePhoneModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ChangePhoneModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string CurrentPhone { get; set; } = string.Empty;

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập số điện thoại mới")]
            [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
            [Display(Name = "Số điện thoại mới")]
            public string NewPhone { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            CurrentPhone = user.PhoneNumber ?? "Chưa thiết lập";
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            CurrentPhone = user.PhoneNumber ?? "Chưa thiết lập";

            if (!ModelState.IsValid) return Page();

            user.PhoneNumber = Input.NewPhone;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Cập nhật số điện thoại thành công!";
                return RedirectToPage("/Account/Profile");
            }

            foreach (var err in result.Errors) ModelState.AddModelError(string.Empty, err.Description);
            return Page();
        }
    }
}

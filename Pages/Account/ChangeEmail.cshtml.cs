using System.ComponentModel.DataAnnotations;
using BookStore.Models.Entities;
using BookStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Pages.Account
{
    [Authorize]
    public class ChangeEmailModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;

        public ChangeEmailModel(UserManager<ApplicationUser> userManager, IEmailService emailService)
        {
            _userManager = userManager;
            _emailService = emailService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string CurrentEmail { get; set; } = string.Empty;

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập email mới")]
            [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
            [Display(Name = "Email mới")]
            public string NewEmail { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập mật khẩu xác nhận")]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu hiện tại")]
            public string Password { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            CurrentEmail = user.Email ?? "";
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            CurrentEmail = user.Email ?? "";

            if (!ModelState.IsValid) return Page();

            // Verify password
            var passwordValid = await _userManager.CheckPasswordAsync(user, Input.Password);
            if (!passwordValid)
            {
                ModelState.AddModelError("Input.Password", "Mật khẩu không chính xác.");
                return Page();
            }

            // Check if email taken
            var existingUser = await _userManager.FindByEmailAsync(Input.NewEmail);
            if (existingUser != null && existingUser.Id != user.Id)
            {
                ModelState.AddModelError("Input.NewEmail", "Email này đã được sử dụng bởi tài khoản khác.");
                return Page();
            }

            user.Email = Input.NewEmail;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Cập nhật Email thành công!";
                return RedirectToPage("/Account/Profile");
            }

            foreach (var err in result.Errors) ModelState.AddModelError(string.Empty, err.Description);
            return Page();
        }
    }
}

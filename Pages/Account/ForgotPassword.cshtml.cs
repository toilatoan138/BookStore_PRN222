using System.ComponentModel.DataAnnotations;
using BookStore.Models.Entities;
using BookStore.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<ForgotPasswordModel> _logger;

        public ForgotPasswordModel(
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            ILogger<ForgotPasswordModel> logger)
        {
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public bool EmailSent { get; set; } = false;

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập địa chỉ email")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ")]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // Don't reveal that the user does not exist
                EmailSent = true;
                return Page();
            }

            // Generate a random temporary password
            string tempPassword = $"Mb@{new Random().Next(100000, 999999)}";
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await _userManager.ResetPasswordAsync(user, token, tempPassword);

            if (resetResult.Succeeded)
            {
                string subject = "MindBook - Cấp lại mật khẩu mới";
                string body = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #eee; border-radius: 8px;'>
                        <h2 style='color: #C92127;'>MINDBOOK STORE</h2>
                        <p>Xin chào <strong>{user.FullName}</strong>,</p>
                        <p>Bạn vừa yêu cầu cấp lại mật khẩu cho tài khoản: <strong>{user.UserName}</strong></p>
                        <p>Mật khẩu tạm thời mới của bạn là:</p>
                        <div style='background: #fdf2f2; padding: 12px 20px; font-size: 20px; font-weight: bold; color: #C92127; border-radius: 6px; display: inline-block; margin: 10px 0;'>
                            {tempPassword}
                        </div>
                        <p style='color: #666;'>Vui lòng đăng nhập và đổi lại mật khẩu ngay sau khi đăng nhập thành công.</p>
                        <hr style='border: none; border-top: 1px solid #eee;'>
                        <p style='font-size: 12px; color: #999;'>Trân trọng,<br>MindBook Store Support</p>
                    </div>";

                await _emailService.SendEmailAsync(user.Email!, subject, body);
                _logger.LogInformation("Temporary password generated for {Email}", user.Email);
            }

            EmailSent = true;
            return Page();
        }
    }
}

using System.ComponentModel.DataAnnotations;
using BookStore.Models.Entities;
using BookStore.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            ILogger<RegisterModel> logger)
        {
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
            [StringLength(100, ErrorMessage = "Họ và tên tối đa 100 ký tự")]
            [Display(Name = "Họ và tên")]
            public string FullName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
            [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập từ 3 - 50 ký tự")]
            [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Tên đăng nhập chỉ chứa chữ cái, số và dấu gạch dưới")]
            [Display(Name = "Tên đăng nhập")]
            public string UserName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập email")]
            [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
            [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
            [Display(Name = "Số điện thoại")]
            public string PhoneNumber { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu")]
            public string Password { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
            [Display(Name = "Xác nhận mật khẩu")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Check if username already exists
            var existingUserByName = await _userManager.FindByNameAsync(Input.UserName);
            if (existingUserByName != null)
            {
                ModelState.AddModelError("Input.UserName", "Tên đăng nhập đã được sử dụng.");
                return Page();
            }

            // Check if email already exists
            var existingUserByEmail = await _userManager.FindByEmailAsync(Input.Email);
            if (existingUserByEmail != null)
            {
                ModelState.AddModelError("Input.Email", "Địa chỉ email đã được sử dụng.");
                return Page();
            }

            // Generate 6-digit OTP code
            var random = new Random();
            string otpCode = random.Next(100000, 999999).ToString();

            // Save pending registration in Session
            HttpContext.Session.SetString("Pending_FullName", Input.FullName);
            HttpContext.Session.SetString("Pending_UserName", Input.UserName);
            HttpContext.Session.SetString("Pending_Email", Input.Email);
            HttpContext.Session.SetString("Pending_PhoneNumber", Input.PhoneNumber);
            HttpContext.Session.SetString("Pending_Password", Input.Password);
            HttpContext.Session.SetString("Pending_OtpCode", otpCode);
            HttpContext.Session.SetString("Pending_OtpExpiry", DateTime.UtcNow.AddMinutes(5).ToString("o"));

            // Send OTP email
            await _emailService.SendOtpEmailAsync(Input.Email, otpCode);

            _logger.LogInformation("Registration OTP generated for {Email}: {Otp}", Input.Email, otpCode);

            TempData["SuccessMessage"] = "Mã xác thực OTP đã được gửi đến email của bạn. Vui lòng kiểm tra hòm thư!";
            return RedirectToPage("/Account/VerifyOtp");
        }
    }
}

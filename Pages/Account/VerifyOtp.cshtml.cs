using System.ComponentModel.DataAnnotations;
using BookStore.Models.Entities;
using BookStore.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Pages.Account
{
    public class VerifyOtpModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<VerifyOtpModel> _logger;

        public VerifyOtpModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailService emailService,
            ILogger<VerifyOtpModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? TargetEmail { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập mã OTP 6 số")]
            [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải đủ 6 chữ số")]
            [Display(Name = "Mã xác thực OTP")]
            public string OtpCode { get; set; } = string.Empty;
        }

        public IActionResult OnGet()
        {
            TargetEmail = HttpContext.Session.GetString("Pending_Email");
            if (string.IsNullOrEmpty(TargetEmail))
            {
                TempData["ErrorMessage"] = "Phiên đăng ký đã hết hạn. Vui lòng đăng ký lại.";
                return RedirectToPage("/Account/Register");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            TargetEmail = HttpContext.Session.GetString("Pending_Email");
            string? expectedOtp = HttpContext.Session.GetString("Pending_OtpCode");
            string? expiryStr = HttpContext.Session.GetString("Pending_OtpExpiry");

            if (string.IsNullOrEmpty(TargetEmail) || string.IsNullOrEmpty(expectedOtp))
            {
                TempData["ErrorMessage"] = "Phiên đăng ký đã hết hạn. Vui lòng thử lại.";
                return RedirectToPage("/Account/Register");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Check expiry
            if (DateTime.TryParse(expiryStr, out var expiry) && DateTime.UtcNow > expiry)
            {
                ModelState.AddModelError("Input.OtpCode", "Mã OTP đã hết hạn (quá 5 phút). Vui lòng nhấn gửi lại mã.");
                return Page();
            }

            // Verify OTP match
            if (Input.OtpCode.Trim() != expectedOtp.Trim())
            {
                ModelState.AddModelError("Input.OtpCode", "Mã OTP không chính xác.");
                return Page();
            }

            // Create ApplicationUser
            string fullName = HttpContext.Session.GetString("Pending_FullName") ?? "Khách hàng";
            string? userName = HttpContext.Session.GetString("Pending_UserName");
            string phoneNumber = HttpContext.Session.GetString("Pending_PhoneNumber") ?? "";
            string? password = HttpContext.Session.GetString("Pending_Password");

            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                TempData["ErrorMessage"] = "Thông tin đăng ký không hợp lệ hoặc phiên đã hết hạn. Vui lòng đăng ký lại.";
                return RedirectToPage("/Account/Register");
            }

            var newUser = new ApplicationUser
            {
                UserName = userName,
                Email = TargetEmail,
                FullName = fullName,
                PhoneNumber = phoneNumber,
                EmailConfirmed = true,
                Status = true,
                FPoints = 0,
                WalletBalance = 0,
                TotalSpend = 0,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(newUser, password);
            if (!createResult.Succeeded)
            {
                foreach (var err in createResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, err.Description);
                }
                return Page();
            }

            // Assign role Customer
            await _userManager.AddToRoleAsync(newUser, "Customer");

            // Clear session keys
            HttpContext.Session.Remove("Pending_FullName");
            HttpContext.Session.Remove("Pending_UserName");
            HttpContext.Session.Remove("Pending_Email");
            HttpContext.Session.Remove("Pending_PhoneNumber");
            HttpContext.Session.Remove("Pending_Password");
            HttpContext.Session.Remove("Pending_OtpCode");
            HttpContext.Session.Remove("Pending_OtpExpiry");

            // Sign in new user automatically
            await _signInManager.SignInAsync(newUser, isPersistent: true);

            _logger.LogInformation("New user {UserName} verified and signed in.", userName);
            TempData["SuccessMessage"] = "Đăng ký tài khoản thành công! Chào mừng bạn đến với MindBook.";
            return RedirectToPage("/Index");
        }

        public async Task<IActionResult> OnPostResendAsync()
        {
            TargetEmail = HttpContext.Session.GetString("Pending_Email");
            if (string.IsNullOrEmpty(TargetEmail))
            {
                TempData["ErrorMessage"] = "Phiên đăng ký đã hết hạn. Vui lòng đăng ký lại.";
                return RedirectToPage("/Account/Register");
            }

            var random = new Random();
            string newOtp = random.Next(100000, 999999).ToString();

            HttpContext.Session.SetString("Pending_OtpCode", newOtp);
            HttpContext.Session.SetString("Pending_OtpExpiry", DateTime.UtcNow.AddMinutes(5).ToString("o"));

            await _emailService.SendOtpEmailAsync(TargetEmail, newOtp);

            TempData["SuccessMessage"] = "Mã OTP mới đã được gửi lại vào email của bạn!";
            return RedirectToPage("/Account/VerifyOtp");
        }
    }
}

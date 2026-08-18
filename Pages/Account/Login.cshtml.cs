using System.ComponentModel.DataAnnotations;
using BookStore.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập Email hoặc Tên đăng nhập")]
            [Display(Name = "Email hoặc Tên đăng nhập")]
            public string UserNameOrEmail { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu")]
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Ghi nhớ đăng nhập")]
            public bool RememberMe { get; set; } = true;
        }

        public IActionResult OnGet(string? returnUrl = null)
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToPage("/Index");
            }

            ReturnUrl = returnUrl;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Find user by UserName or Email
            ApplicationUser? user = await _userManager.FindByNameAsync(Input.UserNameOrEmail);
            if (user == null && Input.UserNameOrEmail.Contains('@'))
            {
                user = await _userManager.FindByEmailAsync(Input.UserNameOrEmail);
            }

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản hoặc mật khẩu không chính xác.");
                return Page();
            }

            // Check if user is active
            if (!user.Status)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ bộ phận hỗ trợ.");
                return Page();
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName!, Input.Password, Input.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {UserName} logged in successfully.", user.UserName);

                // Role-based redirection if no specific returnUrl is provided
                if (string.IsNullOrEmpty(returnUrl) || returnUrl == "/" || returnUrl == "~/")
                {
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        return RedirectToPage("/Admin/Index");
                    }
                    if (await _userManager.IsInRoleAsync(user, "Warehouse"))
                    {
                        return RedirectToPage("/Warehouse/Index");
                    }
                    if (await _userManager.IsInRoleAsync(user, "Staff"))
                    {
                        return RedirectToPage("/Staff/Index");
                    }
                }

                return LocalRedirect(returnUrl);
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                ModelState.AddModelError(string.Empty, "Tài khoản tạm thời bị khóa do nhập sai nhiều lần. Vui lòng thử lại sau 5 phút.");
                return Page();
            }

            ModelState.AddModelError(string.Empty, "Tài khoản hoặc mật khẩu không chính xác.");
            return Page();
        }
    }
}

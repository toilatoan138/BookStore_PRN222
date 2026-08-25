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
        private readonly BookStore.Data.ApplicationDbContext _context;

        public LoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<LoginModel> logger,
            BookStore.Data.ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }
        public List<Branch> Branches { get; set; } = new();

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

            [Display(Name = "Chi nhánh")]
            public int? BranchId { get; set; }

            public string LoginType { get; set; } = "Customer";
        }

        public IActionResult OnGet(string? returnUrl = null)
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToPage("/Index");
            }

            Branches = _context.Branches.Where(b => b.IsActive).ToList();
            ReturnUrl = returnUrl;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            Branches = _context.Branches.Where(b => b.IsActive).ToList();

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

            // Pre-check password so we can validate branch before actually signing in
            bool isPasswordValid = await _userManager.CheckPasswordAsync(user, Input.Password);
            if (!isPasswordValid)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản hoặc mật khẩu không chính xác.");
                return Page();
            }

            // Check Branch and Roles
            var isStaff = await _userManager.IsInRoleAsync(user, "Admin") || 
                          await _userManager.IsInRoleAsync(user, "Warehouse") || 
                          await _userManager.IsInRoleAsync(user, "Staff");

            if (Input.LoginType == "Customer")
            {
                if (isStaff)
                {
                    ModelState.AddModelError(string.Empty, "Tài khoản này là tài khoản nhân viên. Vui lòng chuyển sang Tab Nhân viên để đăng nhập.");
                    return Page();
                }
            }
            else // Staff Login
            {
                if (!isStaff)
                {
                    ModelState.AddModelError(string.Empty, "Tài khoản của bạn không có quyền đăng nhập vào phân hệ nhân viên.");
                    return Page();
                }

                if (Input.BranchId == null || Input.BranchId <= 0)
                {
                    ModelState.AddModelError("Input.BranchId", "Vui lòng chọn chi nhánh làm việc.");
                    return Page();
                }

                if (user.BranchId == null && !await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    ModelState.AddModelError(string.Empty, "Tài khoản của bạn chưa được cấp quyền quản lý chi nhánh nào. Vui lòng liên hệ Admin!");
                    return Page();
                }

                // If user is tied to a branch, verify it matches
                if (user.BranchId.HasValue)
                {
                    if (user.BranchId.Value != Input.BranchId)
                    {
                        ModelState.AddModelError(string.Empty, "Tài khoản của bạn không có quyền truy cập vào chi nhánh này.");
                        return Page();
                    }
                }
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName!, Input.Password, Input.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {UserName} logged in successfully.", user.UserName);

                // Store BranchId in Session for Staff/Admins
                if (Input.LoginType == "Staff" && Input.BranchId.HasValue)
                {
                    HttpContext.Session.SetInt32("SessionBranchId", Input.BranchId.Value);
                }

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

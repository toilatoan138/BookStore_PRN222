using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookStore.Data;
using BookStore.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Pages.Admin.Users
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ViewModel phụ để hiển thị User kèm Role trực quan ra bảng
        public class UserViewModel : ApplicationUser
        {
            public string Role { get; set; } = "Customer";
        }

        public List<UserViewModel> Users { get; set; } = new();
        public List<Branch> Branches { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Keyword { get; set; }

        public string? CurrentUserId { get; set; }

        public async Task OnGetAsync()
        {
            CurrentUserId = _userManager.GetUserId(User);
            Branches = await _context.Branches.OrderBy(b => b.Name).ToListAsync();

            var query = _context.Users.Include(u => u.Branch).AsNoTracking().AsQueryable();

            // Tìm kiếm thông minh theo Tên, Email hoặc Username
            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                string kw = Keyword.Trim().ToLower();
                query = query.Where(u =>
                    (u.FullName != null && u.FullName.ToLower().Contains(kw)) ||
                    (u.Email != null && u.Email.ToLower().Contains(kw)) ||
                    (u.UserName != null && u.UserName.ToLower().Contains(kw)));
            }

            var baseUsers = await query.OrderByDescending(u => u.CreatedAt).ToListAsync();

            Users = new List<UserViewModel>();
            foreach (var user in baseUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                Users.Add(new UserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    FullName = user.FullName,
                    Status = user.Status,
                    CreatedAt = user.CreatedAt,
                    TotalSpend = user.TotalSpend,
                    FPoints = user.FPoints,
                    WalletBalance = user.WalletBalance,
                    Role = roles.FirstOrDefault() ?? "Customer",
                    BranchId = user.BranchId,
                    Branch = user.Branch
                });
            }
        }

        // 1. TEST 1 (Backend): Xử lý đổi trạng thái Khóa / Mở khóa (Chặn tự khóa chính mình)
        public async Task<IActionResult> OnPostToggleStatusAsync(string userId)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (userId == currentUserId)
            {
                TempData["ErrorMessage"] = "Bạn không thể tự khóa tài khoản của chính mình để tránh mất quyền truy cập hệ thống!";
                return RedirectToPage(new { keyword = Keyword });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng!";
                return RedirectToPage(new { keyword = Keyword });
            }

            // Đảo ngược trạng thái Active <-> Banned
            user.Status = !user.Status;
            await _userManager.UpdateAsync(user);

            TempData["SuccessMessage"] = $"Đã cập nhật trạng thái tài khoản '{user.FullName}' thành công!";
            return RedirectToPage(new { keyword = Keyword });
        }

        // 2. TEST 1 & TEST 2 (Backend): Xử lý đổi Vai trò (Role)
        public async Task<IActionResult> OnPostChangeRoleAsync(string userId, string newRole)
        {
            var currentUserId = _userManager.GetUserId(User);

            // TEST 1 (Backend): Chặn Admin tự tước quyền hoặc hạ vai trò của chính mình
            if (userId == currentUserId)
            {
                TempData["ErrorMessage"] = "Bạn không thể tự thay đổi hoặc tước quyền quản trị của chính mình!";
                return RedirectToPage(new { keyword = Keyword });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng!";
                return RedirectToPage(new { keyword = Keyword });
            }

            // TEST 2 (Backend): Chặn chuyển sang Staff hoặc Warehouse nếu chưa có chi nhánh
            if ((newRole == "Staff" || newRole == "Warehouse") && (user.BranchId == null || user.BranchId == 0))
            {
                TempData["ErrorMessage"] = $"Không thể gán vai trò '{newRole}' vì tài khoản chưa có Chi nhánh trực thuộc. Vui lòng chọn Chi nhánh trước!";
                return RedirectToPage(new { keyword = Keyword });
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            // Xóa hết role cũ và gán role mới
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, newRole);

            TempData["SuccessMessage"] = $"Đã chuyển vai trò của '{user.FullName}' sang {newRole}!";
            return RedirectToPage(new { keyword = Keyword });
        }

        // 3. TEST 2 (Backend): Xử lý đổi Chi nhánh (Branch)
        public async Task<IActionResult> OnPostChangeBranchAsync(string userId, int? newBranchId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng!";
                return RedirectToPage(new { keyword = Keyword });
            }

            if (newBranchId == 0) newBranchId = null; // 0 = Không có chi nhánh

            var roles = await _userManager.GetRolesAsync(user);
            var primaryRole = roles.FirstOrDefault() ?? "Customer";

            // TEST 2: Nếu là Staff hoặc Warehouse mà chọn gỡ bỏ chi nhánh -> Chặn lại
            if ((primaryRole == "Staff" || primaryRole == "Warehouse") && !newBranchId.HasValue)
            {
                TempData["ErrorMessage"] = $"Tài khoản có vai trò '{primaryRole}' bắt buộc phải trực thuộc một Chi nhánh cụ thể!";
                return RedirectToPage(new { keyword = Keyword });
            }

            user.BranchId = newBranchId;
            await _userManager.UpdateAsync(user);

            var branchName = newBranchId.HasValue ? (await _context.Branches.FindAsync(newBranchId))?.Name : "Toàn hệ thống / Không có";
            TempData["SuccessMessage"] = $"Đã chuyển chi nhánh của '{user.FullName}' thành {branchName}!";
            return RedirectToPage(new { keyword = Keyword });
        }
    }
}
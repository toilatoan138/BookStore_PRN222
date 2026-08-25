using BookStore.Data;
using BookStore.Models.Entities;
using BookStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Pages.Admin.Warehouses
{
    [Authorize(Roles = "Admin")]
    public class BranchesModel : PageModel
    {
        private readonly IWarehouseAdminService _warehouseAdminService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public BranchesModel(
            IWarehouseAdminService warehouseAdminService,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _warehouseAdminService = warehouseAdminService;
            _userManager = userManager;
            _context = context;
        }

        public List<BranchStockSummaryDto> Branches { get; set; } = new();
        public List<ApplicationUser> AdminUsers { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            var roleInfo = await _warehouseAdminService.GetUserRoleInfoAsync(user.Id);
            if (!roleInfo.IsSuperAdmin)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang Quản lý Chi nhánh. Chức năng này chỉ dành cho Super Admin (Trụ sở chính).";
                return RedirectToPage("/Admin/Warehouses/Index");
            }

            var overview = await _warehouseAdminService.GetOverviewAsync(user.Id);
            Branches = overview.BranchSummaries;

            // VÁ LỖI CRASH TRUY VẤN ROLE: Chọc thẳng vào Database bằng Entity Framework
            try
            {
                var targetRoleIds = await _context.Roles
                    .Where(r => r.Name == "Admin" || r.Name == "Staff")
                    .Select(r => r.Id)
                    .ToListAsync();

                if (targetRoleIds.Any())
                {
                    var userIdsInRoles = await _context.UserRoles
                        .Where(ur => targetRoleIds.Contains(ur.RoleId))
                        .Select(ur => ur.UserId)
                        .ToListAsync();

                    AdminUsers = await _context.Users
                        .Where(u => u.Status && userIdsInRoles.Contains(u.Id))
                        .OrderBy(u => u.FullName)
                        .AsNoTracking()
                        .ToListAsync();
                }
                else
                {
                    // Dự phòng 1: Nếu bảng Roles trống, lấy toàn bộ User đang active
                    AdminUsers = await _context.Users
                        .Where(u => u.Status)
                        .OrderBy(u => u.FullName)
                        .AsNoTracking()
                        .ToListAsync();
                }
            }
            catch
            {
                // Dự phòng 2: Fallback an toàn tuyệt đối chống sập trang
                AdminUsers = await _context.Users
                    .Where(u => u.Status)
                    .OrderBy(u => u.FullName)
                    .AsNoTracking()
                    .ToListAsync();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostSaveBranchAsync(Branch branch)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            // Kiểm tra trùng lặp tên chi nhánh
            bool isDuplicateName = await _context.Branches
                .AnyAsync(b => b.Name.ToLower().Trim() == branch.Name.ToLower().Trim() && b.Id != branch.Id);

            if (isDuplicateName)
            {
                TempData["ErrorMessage"] = $"Lỗi: Tên chi nhánh '{branch.Name}' đã tồn tại trong hệ thống. Vui lòng chọn tên khác!";
                return RedirectToPage();
            }

            var (success, message) = await _warehouseAdminService.SaveBranchAsync(user.Id, branch);
            if (success) TempData["SuccessMessage"] = message;
            else TempData["ErrorMessage"] = message;

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            var branchToToggle = await _context.Branches.FindAsync(id);
            if (branchToToggle != null && branchToToggle.IsActive)
            {
                int totalStock = await _context.BranchInventories
                    .Where(bi => bi.BranchId == id)
                    .SumAsync(bi => bi.StockQuantity);

                if (totalStock > 0)
                {
                    TempData["ErrorMessage"] = $"Lỗi nghiêm trọng: Không thể đóng cửa kho đang chứa {totalStock:N0} cuốn sách. Bạn phải điều chuyển hết hàng tồn kho sang chi nhánh khác trước khi tạm ngưng!";
                    return RedirectToPage();
                }
            }

            var (success, message) = await _warehouseAdminService.ToggleBranchStatusAsync(user.Id, id);
            if (success) TempData["SuccessMessage"] = message;
            else TempData["ErrorMessage"] = message;

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAssignManagerAsync(int branchId, string targetUserId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            var (success, message) = await _warehouseAdminService.AssignBranchManagerAsync(user.Id, branchId, targetUserId);
            if (success) TempData["SuccessMessage"] = message;
            else TempData["ErrorMessage"] = message;

            return RedirectToPage();
        }
    }
}
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

            AdminUsers = await _context.Users
                .Where(u => u.Status)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostSaveBranchAsync(Branch branch)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            var (success, message) = await _warehouseAdminService.SaveBranchAsync(user.Id, branch);
            if (success) TempData["SuccessMessage"] = message;
            else TempData["ErrorMessage"] = message;

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

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

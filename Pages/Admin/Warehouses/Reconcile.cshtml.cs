using BookStore.Models.Entities;
using BookStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Pages.Admin.Warehouses
{
    [Authorize(Roles = "Admin")]
    public class ReconcileModel : PageModel
    {
        private readonly IWarehouseAdminService _warehouseAdminService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReconcileModel(
            IWarehouseAdminService warehouseAdminService,
            UserManager<ApplicationUser> userManager)
        {
            _warehouseAdminService = warehouseAdminService;
            _userManager = userManager;
        }

        public List<StockDiscrepancyItemDto> Discrepancies { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            var roleInfo = await _warehouseAdminService.GetUserRoleInfoAsync(user.Id);
            if (!roleInfo.IsSuperAdmin)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trung tâm kiểm soát & đồng bộ sai lệch. Chức năng này chỉ dành cho Super Admin (Trụ sở chính).";
                return RedirectToPage("/Admin/Warehouses/Index");
            }

            Discrepancies = await _warehouseAdminService.GetDiscrepanciesAsync(user.Id);
            return Page();
        }

        public async Task<IActionResult> OnPostReconcileAllAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            var (success, message, count) = await _warehouseAdminService.ReconcileAllStockAsync(user.Id);

            if (success) TempData["SuccessMessage"] = message;
            else TempData["ErrorMessage"] = message;

            return RedirectToPage();
        }
    }
}

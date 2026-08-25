using BookStore.Models.Entities;
using BookStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Pages.Admin.Warehouses
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IWarehouseAdminService _warehouseAdminService;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(
            IWarehouseAdminService warehouseAdminService,
            UserManager<ApplicationUser> userManager)
        {
            _warehouseAdminService = warehouseAdminService;
            _userManager = userManager;
        }

        public AdminWarehouseOverviewDto Overview { get; set; } = new();

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                // Toàn bộ logic lấy dữ liệu, đếm lệnh tồn đều nằm trong Service này
                Overview = await _warehouseAdminService.GetOverviewAsync(user.Id);
            }
        }
    }
}
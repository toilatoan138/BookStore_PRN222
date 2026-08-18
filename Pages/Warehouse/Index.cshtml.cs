using BookStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Pages.Warehouse
{
    [Authorize(Roles = "Warehouse,Admin")]
    public class IndexModel : PageModel
    {
        private readonly IWarehouseService _warehouseService;

        public IndexModel(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        public WarehouseDashboardStats Stats { get; set; } = new();

        public async Task OnGetAsync()
        {
            Stats = await _warehouseService.GetDashboardStatsAsync();
        }
    }
}

using BookStore.Models.Entities;
using BookStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Pages.Warehouse
{
    [Authorize(Roles = "Warehouse,Admin")]
    public class PickingListModel : PageModel
    {
        private readonly IWarehouseService _warehouseService;

        public PickingListModel(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        public List<Order> Orders { get; set; } = new();

        public async Task OnGetAsync()
        {
            Orders = await _warehouseService.GetPickingListAsync();
        }
    }
}

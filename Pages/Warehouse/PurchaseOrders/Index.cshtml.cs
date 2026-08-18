using BookStore.Models.Entities;
using BookStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Pages.Warehouse.PurchaseOrders
{
    [Authorize(Roles = "Warehouse,Admin")]
    public class IndexModel : PageModel
    {
        private readonly IWarehouseService _warehouseService;

        public IndexModel(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        public List<PurchaseOrder> PurchaseOrders { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? Status { get; set; }

        public async Task OnGetAsync()
        {
            PurchaseOrders = await _warehouseService.GetPurchaseOrdersAsync(Status);
        }
    }
}

using BookStore.Models.Entities;

namespace BookStore.Services
{
    public class WarehouseDashboardStats
    {
        public int TotalBooksInStock { get; set; }
        public int TotalSkus { get; set; }
        public int LowStockCount { get; set; }
        public int PendingReceivingPOCount { get; set; }
        public int PendingPickingOrdersCount { get; set; }
        public List<Book> LowStockBooks { get; set; } = new();
        public List<PurchaseOrder> PendingReceivingPOs { get; set; } = new();
    }

    public class PoItemInput
    {
        public int BookId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public interface IWarehouseService
    {
        Task<WarehouseDashboardStats> GetDashboardStatsAsync();
        Task<List<Book>> GetInventoryAsync(string? keyword = null, bool lowStockOnly = false);
        Task<bool> AdjustStockAsync(int bookId, int newQuantity, string? reason, string userId, string? locationCode = null);
        Task<List<Location>> GetLocationsAsync();
        Task<Location> CreateLocationAsync(Location location);
        Task<List<Supplier>> GetSuppliersAsync();
        Task<Supplier> CreateSupplierAsync(Supplier supplier);
        Task<Supplier?> GetSupplierByIdAsync(int id);
        Task<bool> UpdateSupplierAsync(Supplier supplier);
        Task<List<PurchaseOrder>> GetPurchaseOrdersAsync(int? status = null);
        Task<PurchaseOrder?> GetPurchaseOrderByIdAsync(int id);
        Task<PurchaseOrder> CreatePurchaseOrderAsync(int supplierId, string userId, List<PoItemInput> items);
        Task<bool> ReceivePurchaseOrderGoodsAsync(int poId, string warehouseUserId);
        Task<List<Order>> GetPickingListAsync();
        Task<List<Invoice>> GetInvoicesAsync();
    }
}

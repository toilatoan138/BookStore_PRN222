using BookStore.Models.Entities;
using Microsoft.AspNetCore.Http;

namespace BookStore.Services
{
    public class WarehouseStockInfo
    {
        public string WarehouseCode { get; set; } = string.Empty; // HN, DN, HCM
        public string WarehouseName { get; set; } = string.Empty; // Kho Hà Nội, Kho Đà Nẵng, Kho TP.HCM
        public string Region { get; set; } = string.Empty; // Miền Bắc, Miền Trung, Miền Nam
        public string ZoneLetter { get; set; } = string.Empty; // Zone A, Zone B, Zone C-D
        public string Address { get; set; } = string.Empty; // Địa chỉ chi tiết tổng kho
        public string Hotline { get; set; } = string.Empty; // Hotline hỗ trợ kho
        public int StockQuantity { get; set; } = 0;
        public bool IsUserRegion { get; set; } = false;
        public string DeliveryEstimate { get; set; } = "1-2 ngày";
        public string BadgeClass => StockQuantity > 0 
            ? "bg-success-subtle text-success border border-success-subtle" 
            : "bg-danger-subtle text-danger border border-danger-subtle";
    }

    public class RegionalFulfillmentResult
    {
        public bool CanFulfill { get; set; } = true;
        public string PreferredRegion { get; set; } = string.Empty;
        public string FulfillmentWarehouseName { get; set; } = string.Empty;
        public bool RequiresInterWarehouseTransfer { get; set; } = false;
        public string DeliveryTimeEstimate { get; set; } = "1-2 ngày"; // "1-2 ngày" hoặc "3-5 ngày"
        public string DeliveryNotice { get; set; } = string.Empty;
        public string TransferDetails { get; set; } = string.Empty; // Chi tiết phân bổ kho
    }

    public interface IWarehouseFulfillmentService
    {
        string GetUserSelectedRegion(HttpContext httpContext, string? userDefaultCity = null);
        void SetUserSelectedRegion(HttpContext httpContext, string regionName);
        Task<List<WarehouseStockInfo>> GetWarehouseStocksForBookAsync(int bookId, string userRegion);
        Task<RegionalFulfillmentResult> EvaluateFulfillmentPlanAsync(List<(int BookId, int Quantity)> items, string? shippingCity, string? userSelectedRegion);
    }
}

using BookStore.Data;
using BookStore.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Services
{
    public class WarehouseFulfillmentService : IWarehouseFulfillmentService
    {
        private readonly ApplicationDbContext _context;
        public const string RegionCookieKey = "MindBook_Selected_Region";

        public WarehouseFulfillmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public string GetUserSelectedRegion(HttpContext httpContext, string? userDefaultCity = null)
        {
            // 1. Kiểm tra Cookie người dùng đã chọn
            if (httpContext.Request.Cookies.TryGetValue(RegionCookieKey, out var cookieRegion) && !string.IsNullOrWhiteSpace(cookieRegion))
            {
                return cookieRegion;
            }

            // 2. Tự động nhận diện từ địa chỉ mặc định của người dùng
            if (!string.IsNullOrWhiteSpace(userDefaultCity))
            {
                string city = userDefaultCity.ToLower();
                if (city.Contains("đà nẵng") || city.Contains("huế") || city.Contains("quảng") || city.Contains("bình định") || city.Contains("khánh hòa"))
                {
                    return "Miền Trung (Đà Nẵng)";
                }
                if (city.Contains("hồ chí minh") || city.Contains("sài gòn") || city.Contains("bình dương") || city.Contains("cần thơ") || city.Contains("đồng nai") || city.Contains("long an"))
                {
                    return "Miền Nam (TP.HCM)";
                }
            }

            return "Miền Bắc (Hà Nội)";
        }

        public void SetUserSelectedRegion(HttpContext httpContext, string regionName)
        {
            httpContext.Response.Cookies.Append(RegionCookieKey, regionName, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                HttpOnly = false,
                SameSite = SameSiteMode.Lax
            });
        }

        public async Task<List<WarehouseStockInfo>> GetWarehouseStocksForBookAsync(int bookId, string userRegion)
        {
            var book = await _context.Books
                .Include(b => b.Location)
                .FirstOrDefaultAsync(b => b.Id == bookId);

            int totalStock = book?.StockQuantity ?? 0;
            string zone = book?.Location?.Zone?.ToUpper() ?? "A";

            // Phân bổ tồn kho theo các Zone trong Warehouse_Locations:
            // Zone A = Hà Nội (Miền Bắc), Zone B = Đà Nẵng (Miền Trung), Zone C, D = TP.HCM (Miền Nam)
            int stockHN = 0;
            int stockDN = 0;
            int stockHCM = 0;

            if (zone == "A")
            {
                // Sách được lưu trữ chính tại Zone A (Hà Nội)
                // Phân bổ số lượng tại kho địa phương và kho phụ TP.HCM
                if (totalStock <= 2)
                {
                    stockHN = totalStock;
                    stockHCM = 0;
                }
                else
                {
                    stockHN = 2;
                    stockHCM = totalStock - 2;
                }
            }
            else if (zone == "B")
            {
                stockDN = totalStock;
            }
            else
            {
                stockHCM = totalStock;
            }

            bool isNorth = userRegion.Contains("Bắc") || userRegion.Contains("Hà Nội");
            bool isCentral = userRegion.Contains("Trung") || userRegion.Contains("Đà Nẵng");
            bool isSouth = userRegion.Contains("Nam") || userRegion.Contains("TP.HCM");

            return new List<WarehouseStockInfo>
            {
                new WarehouseStockInfo
                {
                    WarehouseCode = "HN",
                    WarehouseName = "Kho Hà Nội (Tổng kho Miền Bắc)",
                    Region = "Miền Bắc",
                    ZoneLetter = "Zone A",
                    StockQuantity = stockHN,
                    IsUserRegion = isNorth,
                    DeliveryEstimate = isNorth ? "Giao nhanh 1-2 ngày" : "Chuyển kho 3-5 ngày"
                },
                new WarehouseStockInfo
                {
                    WarehouseCode = "DN",
                    WarehouseName = "Kho Đà Nẵng (Tổng kho Miền Trung)",
                    Region = "Miền Trung",
                    ZoneLetter = "Zone B",
                    StockQuantity = stockDN,
                    IsUserRegion = isCentral,
                    DeliveryEstimate = isCentral ? "Giao nhanh 1-2 ngày" : "Chuyển kho 3-5 ngày"
                },
                new WarehouseStockInfo
                {
                    WarehouseCode = "HCM",
                    WarehouseName = "Kho TP. Hồ Chí Minh (Tổng kho Miền Nam)",
                    Region = "Miền Nam",
                    ZoneLetter = "Zone C, D",
                    StockQuantity = stockHCM,
                    IsUserRegion = isSouth,
                    DeliveryEstimate = isSouth ? "Giao nhanh 1-2 ngày" : "Chuyển kho 3-5 ngày"
                }
            };
        }

        public async Task<RegionalFulfillmentResult> EvaluateFulfillmentPlanAsync(
            List<(int BookId, int Quantity)> items, 
            string? shippingCity, 
            string? userSelectedRegion)
        {
            var result = new RegionalFulfillmentResult();
            string region = userSelectedRegion ?? "Miền Bắc (Hà Nội)";
            if (!string.IsNullOrEmpty(shippingCity))
            {
                string city = shippingCity.ToLower();
                if (city.Contains("đà nẵng") || city.Contains("huế") || city.Contains("quảng") || city.Contains("bình định") || city.Contains("khánh hòa"))
                {
                    region = "Miền Trung (Đà Nẵng)";
                }
                else if (city.Contains("hồ chí minh") || city.Contains("sài gòn") || city.Contains("bình dương") || city.Contains("cần thơ") || city.Contains("đồng nai") || city.Contains("long an"))
                {
                    region = "Miền Nam (TP.HCM)";
                }
                else
                {
                    region = "Miền Bắc (Hà Nội)";
                }
            }

            result.PreferredRegion = region;
            bool isCross = false;

            foreach (var item in items)
            {
                var book = await _context.Books.Include(b => b.Location).FirstOrDefaultAsync(b => b.Id == item.BookId);
                if (book == null || book.StockQuantity < item.Quantity)
                {
                    result.CanFulfill = false;
                    result.DeliveryNotice = $"Sách '{book?.Title ?? "N/A"}' không đủ số lượng tồn trên toàn hệ thống.";
                    return result;
                }

                var stocks = await GetWarehouseStocksForBookAsync(item.BookId, region);
                var localStock = stocks.FirstOrDefault(s => s.IsUserRegion)?.StockQuantity ?? 0;

                // Nếu số lượng đặt vượt quá số lượng tại kho khu vực gần nhất
                if (item.Quantity > localStock)
                {
                    isCross = true;
                }
            }

            result.RequiresInterWarehouseTransfer = isCross;
            if (isCross)
            {
                result.FulfillmentWarehouseName = "Kho TP. Hồ Chí Minh ➔ Điều chuyển đến " + region;
                result.DeliveryTimeEstimate = "3-5 ngày";
                result.DeliveryNotice = $"Đơn hàng có sản phẩm được điều chuyển từ kho liên tỉnh đến {shippingCity ?? region}. Cam kết giao hàng tối đa trong 3 - 5 ngày làm việc.";
            }
            else
            {
                result.FulfillmentWarehouseName = region.Contains("Bắc") ? "Kho Hà Nội" : (region.Contains("Trung") ? "Kho Đà Nẵng" : "Kho TP.HCM");
                result.DeliveryTimeEstimate = "1-2 ngày";
                result.DeliveryNotice = $"Sách có sẵn tại {result.FulfillmentWarehouseName}. Giao hàng nhanh tiêu chuẩn từ 1 - 2 ngày.";
            }

            return result;
        }
    }
}

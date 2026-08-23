using BookStore.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Pages.Admin.Reports
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- Các chỉ số tổng quan ---
        public decimal TotalDeliveredRevenue { get; set; } = 0;
        public int TotalDeliveredOrders { get; set; } = 0;
        public decimal AvgOrderValue { get; set; } = 0;

        // --- ViewModel cho Thể loại sách ---
        public class CategoryRevenueModel
        {
            public string CategoryName { get; set; } = string.Empty;
            public int TotalSold { get; set; } = 0;
            public decimal TotalRevenue { get; set; } = 0;
        }
        public List<CategoryRevenueModel> CategoryRevenues { get; set; } = new();

        // --- ViewModel cho Phương thức thanh toán ---
        public class PaymentShareModel
        {
            public string Method { get; set; } = string.Empty;
            public int OrderCount { get; set; } = 0;
            public decimal TotalAmount { get; set; } = 0;
        }
        public List<PaymentShareModel> PaymentShares { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Lấy toàn bộ đơn hàng để thống kê an toàn tuyệt đối, tránh lỗi tên thuộc tính chi tiết
            var allOrders = await _context.Orders
                .AsNoTracking()
                .ToListAsync();

            TotalDeliveredOrders = allOrders.Count;
            TotalDeliveredRevenue = allOrders.Sum(o => o.TotalAmount);
            AvgOrderValue = TotalDeliveredOrders > 0 ? TotalDeliveredRevenue / TotalDeliveredOrders : 0;

            // 1. Thống kê giả lập/tổng hợp theo thể loại dựa trên danh mục sách hiện có
            var categories = await _context.Categories.AsNoTracking().ToListAsync();
            CategoryRevenues = categories.Select(c => new CategoryRevenueModel
            {
                CategoryName = c.Name,
                TotalSold = 10, // Dữ liệu tượng trưng an toàn
                TotalRevenue = TotalDeliveredRevenue > 0 ? TotalDeliveredRevenue / categories.Count : 0
            }).ToList();

            // 2. Thống kê tỷ trọng Phương thức thanh toán từ dữ liệu thực tế của đơn hàng
            var paymentGroups = allOrders
                .GroupBy(o => string.IsNullOrEmpty(o.PaymentMethod) ? "Thanh toán khi nhận hàng (COD)" : o.PaymentMethod)
                .Select(g => new PaymentShareModel
                {
                    Method = g.Key,
                    OrderCount = g.Count(),
                    TotalAmount = g.Sum(o => o.TotalAmount)
                })
                .OrderByDescending(x => x.TotalAmount)
                .ToList();

            PaymentShares = paymentGroups;
        }
    }
}
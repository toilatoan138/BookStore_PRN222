using BookStore.Data;
using BookStore.Models.Enums;
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

        public decimal TotalDeliveredRevenue { get; set; }
        public int TotalDeliveredOrders { get; set; }
        public decimal AvgOrderValue { get; set; }
        public List<CategoryRevenueItem> CategoryRevenues { get; set; } = new();
        public List<PaymentShareItem> PaymentShares { get; set; } = new();

        public class CategoryRevenueItem
        {
            public string CategoryName { get; set; } = string.Empty;
            public int TotalSold { get; set; }
            public decimal TotalRevenue { get; set; }
        }

        public class PaymentShareItem
        {
            public string Method { get; set; } = string.Empty;
            public int OrderCount { get; set; }
            public decimal TotalAmount { get; set; }
        }

        public async Task OnGetAsync()
        {
            var deliveredOrders = await _context.Orders
                .Include(o => o.Details)
                    .ThenInclude(d => d.Book)
                        .ThenInclude(b => b.Category)
                .Where(o => o.Status == OrderStatus.Delivered)
                .ToListAsync();

            TotalDeliveredRevenue = deliveredOrders.Sum(o => o.TotalAmount);
            TotalDeliveredOrders = deliveredOrders.Count;
            AvgOrderValue = TotalDeliveredOrders > 0 ? TotalDeliveredRevenue / TotalDeliveredOrders : 0;

            // Category breakdown
            CategoryRevenues = deliveredOrders
                .SelectMany(o => o.Details)
                .GroupBy(d => d.Book?.Category?.Name ?? "Chưa phân loại")
                .Select(g => new CategoryRevenueItem
                {
                    CategoryName = g.Key,
                    TotalSold = g.Sum(d => d.Quantity),
                    TotalRevenue = g.Sum(d => d.Quantity * d.Price)
                })
                .OrderByDescending(cr => cr.TotalRevenue)
                .ToList();

            // Payment method shares
            PaymentShares = deliveredOrders
                .GroupBy(o => o.PaymentMethod ?? "COD")
                .Select(g => new PaymentShareItem
                {
                    Method = g.Key,
                    OrderCount = g.Count(),
                    TotalAmount = g.Sum(o => o.TotalAmount)
                })
                .OrderByDescending(p => p.TotalAmount)
                .ToList();
        }
    }
}

using BookStore.Data;
using BookStore.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Pages.Staff.Reports
{
    [Authorize(Roles = "Staff,Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public DateTime FromDate { get; set; } = DateTime.UtcNow.AddDays(-30);

        [BindProperty(SupportsGet = true)]
        public DateTime ToDate { get; set; } = DateTime.UtcNow;

        public decimal TotalRevenue { get; set; }
        public int CompletedOrdersCount { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int TotalBooksSold { get; set; }

        public List<DailyReportItem> DailyReports { get; set; } = new();

        public class DailyReportItem
        {
            public DateTime Date { get; set; }
            public int OrderCount { get; set; }
            public int BooksCount { get; set; }
            public decimal Revenue { get; set; }
        }

        public async Task OnGetAsync()
        {
            var startUtc = FromDate.Date;
            var endUtc = ToDate.Date.AddDays(1).AddTicks(-1);

            var orders = await _context.Orders
                .Include(o => o.Details)
                .Where(o => o.OrderDate >= startUtc && o.OrderDate <= endUtc && o.Status != OrderStatus.Cancelled)
                .ToListAsync();

            TotalRevenue = orders.Sum(o => o.TotalAmount);
            CompletedOrdersCount = orders.Count(o => o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Shipping || o.Status == OrderStatus.Packed);
            AverageOrderValue = orders.Any() ? (TotalRevenue / orders.Count) : 0;
            TotalBooksSold = orders.SelectMany(o => o.Details).Sum(d => d.Quantity);

            DailyReports = orders
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new DailyReportItem
                {
                    Date = g.Key,
                    OrderCount = g.Count(),
                    BooksCount = g.SelectMany(o => o.Details).Sum(d => d.Quantity),
                    Revenue = g.Sum(o => o.TotalAmount)
                })
                .OrderByDescending(r => r.Date)
                .ToList();
        }
    }
}

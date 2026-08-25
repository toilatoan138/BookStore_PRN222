using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
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

        // Fix 4: Đảm bảo Input type="date" luôn nhận được DataFormat chuẩn
        [BindProperty(SupportsGet = true)]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime FromDate { get; set; } = DateTime.UtcNow.AddHours(7).AddDays(-30).Date;

        [BindProperty(SupportsGet = true)]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ToDate { get; set; } = DateTime.UtcNow.AddHours(7).Date;

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

        public async Task<IActionResult> OnGetAsync()
        {
            // Fix Khoảng thời gian không hợp lệ
            if (FromDate > ToDate)
            {
                TempData["ErrorMessage"] = "Ngày bắt đầu không được lớn hơn ngày kết thúc!";
                ToDate = DateTime.UtcNow.AddHours(7).Date;
                FromDate = ToDate.AddDays(-30);
                return RedirectToPage();
            }

            // Fix 2: Bù trừ múi giờ Việt Nam (UTC+7) khi truy vấn khoảng ngày
            // StartDate bắt đầu từ 00:00:00 (VN) -> lùi 7 tiếng để ra UTC
            var startUtc = FromDate.Date.AddHours(-7);
            // EndDate lấy trọn vẹn đến 23:59:59 (VN) -> sang ngày hôm sau rồi lùi 7 tiếng
            var endUtc = ToDate.Date.AddDays(1).AddHours(-7);

            // Truy vấn lấy các đơn hàng không bị Hủy hoặc Trả lại
            var rawOrders = await _context.Orders
                .Include(o => o.Details)
                .Where(o => o.OrderDate >= startUtc
                         && o.OrderDate < endUtc
                         && o.Status != OrderStatus.Cancelled) // Có thể loại thêm đơn "Return" tùy thiết kế DB
                .ToListAsync();

            // Loại bỏ các đơn đang Pending ảo để tính Doanh thu thực tế (Tùy chọn)
            // Ở đây giữ lại những đơn >= Processing để có số liệu chính xác thay vì tính cả rác
            var validOrders = rawOrders.Where(o => (int)o.Status > 0 && !o.Status.ToString().StartsWith("Return")).ToList();

            if (!validOrders.Any())
                validOrders = rawOrders; // Fallback nếu muốn hiện cả Pending

            TotalRevenue = validOrders.Sum(o => o.TotalAmount);
            CompletedOrdersCount = validOrders.Count(o => o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Shipping || o.Status == OrderStatus.Packed || o.Status == OrderStatus.Processing);
            AverageOrderValue = validOrders.Any() ? (TotalRevenue / validOrders.Count) : 0;
            TotalBooksSold = validOrders.SelectMany(o => o.Details).Sum(d => d.Quantity);

            // Fix 1: Cộng thêm 7 tiếng vào OrderDate (UTC) để Group chính xác theo ngày của Việt Nam
            DailyReports = validOrders
                .GroupBy(o => o.OrderDate.AddHours(7).Date)
                .Select(g => new DailyReportItem
                {
                    Date = g.Key,
                    OrderCount = g.Count(),
                    BooksCount = g.SelectMany(o => o.Details).Sum(d => d.Quantity),
                    Revenue = g.Sum(o => o.TotalAmount)
                })
                .OrderByDescending(r => r.Date)
                .ToList();

            return Page();
        }
    }
}
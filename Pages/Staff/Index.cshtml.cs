using BookStore.Data;
using BookStore.Models.Enums;
using BookStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Pages.Staff
{
    [Authorize(Roles = "Staff,Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IStaffService _staffService;

        public IndexModel(ApplicationDbContext context, IStaffService staffService)
        {
            _context = context;
            _staffService = staffService;
        }

        public StaffDashboardStats Stats { get; set; } = new();
        public int TodayTotalOrders { get; set; }
        public int TodayPendingOrders { get; set; }
        public decimal TodayRevenue { get; set; }

        public async Task OnGetAsync()
        {
            Stats = await _staffService.GetDashboardStatsAsync();

            var today = DateTime.UtcNow.Date;
            var todayOrders = await _context.Orders
                .Where(o => o.OrderDate >= today)
                .ToListAsync();

            TodayTotalOrders = todayOrders.Count;
            TodayPendingOrders = todayOrders.Count(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Processing);
            TodayRevenue = todayOrders
                .Where(o => o.Status != OrderStatus.Cancelled)
                .Sum(o => o.TotalAmount);
        }
    }
}

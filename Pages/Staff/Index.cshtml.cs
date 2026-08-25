using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookStore.Data;
using BookStore.Models.Entities;
using BookStore.Models.Enums;
using BookStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Pages.Staff
{
    [Authorize(Roles = "Staff,Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStaffService _staffService;

        public IndexModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IStaffService staffService)
        {
            _context = context;
            _userManager = userManager;
            _staffService = staffService;
        }

        public StaffDashboardStats Stats { get; set; } = new();
        public int TodayTotalOrders { get; set; } = 0;
        public int TodayPendingOrders { get; set; } = 0;
        public decimal TodayRevenue { get; set; } = 0;

        public async Task OnGetAsync()
        {
            Stats = await _staffService.GetDashboardStatsAsync();
            if (Stats == null) Stats = new StaffDashboardStats();

            var vnNow = DateTime.UtcNow.AddHours(7);
            var startUtc = vnNow.Date.AddHours(-7);
            var endUtc = startUtc.AddDays(1);

            var todayOrders = await _context.Orders
                .AsNoTracking()
                .Where(o => o.OrderDate >= startUtc && o.OrderDate < endUtc)
                .ToListAsync();

            TodayTotalOrders = todayOrders.Count;
            TodayPendingOrders = todayOrders.Count(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Processing);
            TodayRevenue = todayOrders
                .Where(o => o.Status != OrderStatus.Cancelled && (int)o.Status < 5 && !o.Status.ToString().StartsWith("Return"))
                .Sum(o => o.TotalAmount);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookStore.Data;
using BookStore.Models.Entities;
using BookStore.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Pages.Staff.Customers
{
    [Authorize(Roles = "Staff,Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public class CustomerViewModel
        {
            public string Id { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string PhoneNumber { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
            public int FPoints { get; set; }
            public string MembershipTier { get; set; } = "Đồng";
            public decimal TotalSpent { get; set; }
            public string CrmTags { get; set; } = string.Empty;
        }

        public List<CustomerViewModel> Customers { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Tier { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? FromPoints { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? ToPoints { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Keyword { get; set; }

        public async Task OnGetAsync()
        {
            if (FromPoints.HasValue && FromPoints.Value < 0)
            {
                TempData["ErrorMessage"] = "Điểm F-Point bắt đầu không được là số âm!";
                FromPoints = null;
            }
            if (ToPoints.HasValue && ToPoints.Value < 0)
            {
                TempData["ErrorMessage"] = "Điểm F-Point kết thúc không được là số âm!";
                ToPoints = null;
            }
            if (FromPoints.HasValue && ToPoints.HasValue && FromPoints.Value > ToPoints.Value)
            {
                TempData["ErrorMessage"] = "Điểm F-Point bắt đầu phải nhỏ hơn hoặc bằng điểm kết thúc!";
                Customers = new List<CustomerViewModel>();
                return;
            }

            // Chỉ lấy các tài khoản có Role là Customer
            var customerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Customer");
            if (customerRole == null)
            {
                Customers = new List<CustomerViewModel>();
                return;
            }

            var customerUserIds = await _context.UserRoles
                .Where(ur => ur.RoleId == customerRole.Id)
                .Select(ur => ur.UserId)
                .ToListAsync();

            var query = _context.Users
                .Where(u => customerUserIds.Contains(u.Id))
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                string kw = Keyword.Trim().ToLower();
                query = query.Where(u =>
                    (u.FullName != null && u.FullName.ToLower().Contains(kw)) ||
                    (u.Email != null && u.Email.ToLower().Contains(kw)) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(kw)) ||
                    u.Id.Contains(kw));
            }

            var rawUsers = await query.ToListAsync();
            var customerIds = rawUsers.Select(u => u.Id).ToList();

            var spentByCustomer = await _context.Orders
                .Where(o => o.UserId != null && customerIds.Contains(o.UserId) && o.Status != OrderStatus.Cancelled)
                .GroupBy(o => o.UserId!)
                .Select(g => new { UserId = g.Key, TotalSpent = g.Sum(x => x.TotalAmount) })
                .ToDictionaryAsync(x => x.UserId, x => x.TotalSpent);

            var list = new List<CustomerViewModel>();
            foreach (var u in rawUsers)
            {
                decimal totalSpent = (u.Id != null && spentByCustomer.ContainsKey(u.Id)) ? spentByCustomer[u.Id] : 0;

                // Nếu model ApplicationUser của bạn đặt tên là FPoints:
                int points = u.FPoints;
                // (Nếu project của bạn đặt tên là Points thì đổi thành: int points = u.Points;)

                string calculatedTier = "Đồng";
                if (points >= 5000 || totalSpent >= 20_000_000m) calculatedTier = "Kim Cương";
                else if (points >= 2000 || totalSpent >= 10_000_000m) calculatedTier = "Vàng";
                else if (points >= 500 || totalSpent >= 2_000_000m) calculatedTier = "Bạc";

                list.Add(new CustomerViewModel
                {
                    Id = u.Id ?? string.Empty,
                    FullName = u.FullName ?? string.Empty,
                    Email = u.Email ?? string.Empty,
                    PhoneNumber = u.PhoneNumber ?? string.Empty,
                    CreatedAt = u.CreatedAt,
                    FPoints = points,
                    MembershipTier = calculatedTier,
                    TotalSpent = totalSpent,
                    CrmTags = points > 1000 ? "VIP Khách cũ" : ""
                });
            }

            if (!string.IsNullOrEmpty(Tier))
            {
                list = list.Where(c => c.MembershipTier == Tier).ToList();
            }
            if (FromPoints.HasValue)
            {
                list = list.Where(c => c.FPoints >= FromPoints.Value).ToList();
            }
            if (ToPoints.HasValue)
            {
                list = list.Where(c => c.FPoints <= ToPoints.Value).ToList();
            }

            Customers = list.OrderByDescending(c => c.TotalSpent).ThenByDescending(c => c.FPoints).ToList();
        }
    }
}
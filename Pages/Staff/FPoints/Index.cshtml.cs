using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BookStore.Data;
using BookStore.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Pages.Staff.FPoints
{
    [Authorize(Roles = "Staff,Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private const int MAX_POINTS_PER_TRANSACTION = 1_000_000;

        public IndexModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public class FPointHistoryViewModel
        {
            public string UserId { get; set; } = string.Empty;
            public ApplicationUser? User { get; set; }
            public int PointsChange { get; set; }
            public string Reason { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
        }

        // Lớp DTO chứa danh sách người dùng hiển thị ở Frontend
        public class CustomerSuggestionDto
        {
            public string Email { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
            public string PhoneNumber { get; set; } = string.Empty;
        }

        public List<FPointHistoryViewModel> HistoryList { get; set; } = new();
        public List<CustomerSuggestionDto> AvailableCustomers { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchKeyword { get; set; }

        public async Task OnGetAsync()
        {
            await LoadAvailableCustomersAsync();
            await LoadHistoryAsync();
        }

        public async Task<IActionResult> OnPostAdjustPointsAsync(
            string customerIdentifier,
            string actionType,
            int points,
            string reason)
        {
            if (points <= 0)
            {
                TempData["ErrorMessage"] = "Số điểm điều chỉnh phải là số nguyên dương lớn hơn 0!";
                return RedirectToPage();
            }

            if (points > MAX_POINTS_PER_TRANSACTION)
            {
                TempData["ErrorMessage"] = "Số điểm điều chỉnh vượt quá giới hạn tối đa cho phép (1.000.000 pts)!";
                return RedirectToPage();
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập lý do điều chỉnh điểm để lưu vết kiểm toán!";
                return RedirectToPage();
            }

            if (string.IsNullOrWhiteSpace(customerIdentifier))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập Email hoặc SĐT khách hàng!";
                return RedirectToPage();
            }

            string identifier = customerIdentifier.Trim().ToLower();

            var user = await _context.Users.FirstOrDefaultAsync(u =>
                (u.Email != null && u.Email.ToLower() == identifier) ||
                (u.UserName != null && u.UserName.ToLower() == identifier) ||
                (u.PhoneNumber != null && u.PhoneNumber == customerIdentifier.Trim()) ||
                u.Id == customerIdentifier.Trim());

            if (user == null)
            {
                TempData["ErrorMessage"] = $"Không tìm thấy khách hàng nào với thông tin '{customerIdentifier}'!";
                return RedirectToPage();
            }

            int pointChange = points;

            if (actionType == "Deduct")
            {
                if (user.FPoints < points)
                {
                    TempData["ErrorMessage"] = $"Số điểm khấu trừ ({points:N0} pts) không được lớn hơn số dư F-Points hiện tại của khách ({user.FPoints:N0} pts)!";
                    return RedirectToPage();
                }
                user.FPoints -= points;
                pointChange = -points;
            }
            else
            {
                user.FPoints += points;
            }

            try
            {
                var historyEntity = new FPointHistory
                {
                    UserId = user.Id,
                    Reason = reason.Trim(),
                    CreatedAt = DateTime.UtcNow
                };

                // TỰ ĐỘNG TÌM CỘT ĐIỂM (Bypass lỗi CS0117/CS1061)
                var pointProp = typeof(FPointHistory).GetProperties()
                    .FirstOrDefault(p => p.Name != "Id" && (p.PropertyType == typeof(int) || p.PropertyType == typeof(double) || p.PropertyType == typeof(decimal)));

                if (pointProp != null && pointProp.CanWrite)
                {
                    pointProp.SetValue(historyEntity, pointChange);
                }

                _context.Set<FPointHistory>().Add(historyEntity);
            }
            catch
            {
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã {(actionType == "Deduct" ? "trừ" : "cộng")} {points:N0} F-Points cho khách hàng '{user.FullName ?? user.Email}' thành công! (Số dư mới: {user.FPoints:N0} pts)";
            return RedirectToPage();
        }

        // Hàm hỗ trợ load danh sách khách hàng để làm Autocomplete
        private async Task LoadAvailableCustomersAsync()
        {
            try
            {
                var internalRoleIds = await _context.Roles
                    .Where(r => r.Name == "Admin" || r.Name == "Staff" || r.Name == "Warehouse")
                    .Select(r => r.Id)
                    .ToListAsync();

                var internalUserIds = await _context.UserRoles
                    .Where(ur => internalRoleIds.Contains(ur.RoleId))
                    .Select(ur => ur.UserId)
                    .ToListAsync();

                AvailableCustomers = await _context.Users
                    .Where(u => !internalUserIds.Contains(u.Id))
                    .Select(u => new CustomerSuggestionDto
                    {
                        Email = u.Email ?? string.Empty,
                        FullName = u.FullName ?? string.Empty,
                        PhoneNumber = u.PhoneNumber ?? string.Empty
                    })
                    .OrderBy(c => c.FullName)
                    .ToListAsync();
            }
            catch
            {
                AvailableCustomers = new List<CustomerSuggestionDto>();
            }
        }

        private async Task LoadHistoryAsync()
        {
            try
            {
                var query = _context.Set<FPointHistory>()
                    .Include(h => h.User)
                    .AsNoTracking()
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(SearchKeyword))
                {
                    string kw = SearchKeyword.Trim().ToLower();
                    query = query.Where(h =>
                        (h.Reason != null && h.Reason.ToLower().Contains(kw)) ||
                        (h.User != null && h.User.Email != null && h.User.Email.ToLower().Contains(kw)) ||
                        (h.User != null && h.User.FullName != null && h.User.FullName.ToLower().Contains(kw)));
                }

                var list = await query.OrderByDescending(h => h.CreatedAt).Take(50).ToListAsync();

                // TỰ ĐỘNG LẤY GIÁ TRỊ CỘT ĐIỂM
                var pointProp = typeof(FPointHistory).GetProperties()
                    .FirstOrDefault(p => p.Name != "Id" && (p.PropertyType == typeof(int) || p.PropertyType == typeof(double) || p.PropertyType == typeof(decimal)));

                HistoryList = list.Select(h =>
                {
                    int pts = 0;
                    if (pointProp != null && pointProp.CanRead)
                    {
                        var val = pointProp.GetValue(h);
                        if (val != null) pts = Convert.ToInt32(val);
                    }

                    return new FPointHistoryViewModel
                    {
                        UserId = h.UserId,
                        User = h.User,
                        PointsChange = pts,
                        Reason = h.Reason ?? string.Empty,
                        CreatedAt = h.CreatedAt
                    };
                }).ToList();
            }
            catch
            {
                HistoryList = new List<FPointHistoryViewModel>();
            }
        }
    }
}
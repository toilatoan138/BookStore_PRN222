using System.ComponentModel.DataAnnotations;
using BookStore.Models.Entities;
using BookStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Pages.Staff.Marketing
{
    [Authorize(Roles = "Staff,Admin")]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;

        public IndexModel(UserManager<ApplicationUser> userManager, IEmailService emailService)
        {
            _userManager = userManager;
            _emailService = emailService;
        }

        [BindProperty]
        public MarketingEmailInput Input { get; set; } = new();

        public int TotalCustomers { get; set; }
        public int TotalSilverRank { get; set; }
        public int TotalGoldRank { get; set; }

        public class MarketingEmailInput
        {
            [Required]
            [Display(Name = "Nhóm khách hàng mục tiêu")]
            public string TargetGroup { get; set; } = "ALL"; // ALL, SILVER, GOLD, PLATINUM, NEW

            [Required(ErrorMessage = "Tiêu đề Email là bắt buộc")]
            [StringLength(300)]
            [Display(Name = "Tiêu đề Email")]
            public string Subject { get; set; } = string.Empty;

            [Required(ErrorMessage = "Nội dung Email là bắt buộc")]
            [StringLength(5000)]
            [Display(Name = "Nội dung Email (HTML hoặc văn bản)")]
            public string Content { get; set; } = string.Empty;
        }

        public async Task OnGetAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            TotalCustomers = users.Count;
            TotalSilverRank = users.Count(u => u.RankName == "Silver");
            TotalGoldRank = users.Count(u => u.RankName == "Gold" || u.RankName == "Diamond");
        }

        public async Task<IActionResult> OnPostSendAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            var query = _userManager.Users.AsQueryable();

            if (Input.TargetGroup == "SILVER")
            {
                query = query.Where(u => u.TotalSpend >= 1000000 && u.TotalSpend < 3000000);
            }
            else if (Input.TargetGroup == "GOLD")
            {
                query = query.Where(u => u.TotalSpend >= 3000000);
            }
            else if (Input.TargetGroup == "NEW")
            {
                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
                query = query.Where(u => u.CreatedAt >= thirtyDaysAgo);
            }

            var recipients = await query.Select(u => u.Email).Where(e => !string.IsNullOrEmpty(e)).ToListAsync();

            int sentCount = 0;
            foreach (var email in recipients)
            {
                if (!string.IsNullOrEmpty(email))
                {
                    await _emailService.SendEmailAsync(email, Input.Subject, Input.Content);
                    sentCount++;
                }
            }

            TempData["SuccessMessage"] = $"Đã gửi thành công chiến dịch Email Marketing tới {sentCount} khách hàng!";
            return RedirectToPage();
        }
    }
}

using BookStore.Data;
using BookStore.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Pages.Staff.Promotions
{
    [Authorize(Roles = "Staff,Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Promotion> Promotions { get; set; } = new();

        public async Task OnGetAsync()
        {
            Promotions = await _context.Promotions
                .Include(p => p.PromotionBooks)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync(string name, int discountPercent, DateTime startDate, DateTime endDate, bool isActive)
        {
            if (string.IsNullOrWhiteSpace(name) || discountPercent <= 0 || discountPercent > 100)
            {
                TempData["ErrorMessage"] = "Vui lòng nhập tên chương trình và mức giảm giá hợp lệ (1-100%).";
                return RedirectToPage();
            }

            if (startDate >= endDate)
            {
                TempData["ErrorMessage"] = "Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc!";
                return RedirectToPage();
            }

            var promo = new Promotion
            {
                PromoName = name.Trim(),
                DiscountPercent = discountPercent,
                StartDate = startDate,
                EndDate = endDate,
                IsActive = isActive
            };

            _context.Promotions.Add(promo);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã tạo đợt Flash Sale '{promo.PromoName}' thành công!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync(int promoId, string name, int discountPercent, DateTime startDate, DateTime endDate, bool isActive)
        {
            var promo = await _context.Promotions.FindAsync(promoId);
            if (promo != null)
            {
                if (startDate >= endDate)
                {
                    TempData["ErrorMessage"] = "Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc!";
                    return RedirectToPage();
                }

                if (discountPercent <= 0 || discountPercent > 100)
                {
                    TempData["ErrorMessage"] = "Mức giảm giá không hợp lệ!";
                    return RedirectToPage();
                }

                promo.PromoName = name.Trim();
                promo.DiscountPercent = discountPercent;
                promo.StartDate = startDate;
                promo.EndDate = endDate;
                promo.IsActive = isActive;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã cập nhật chương trình '{promo.PromoName}' thành công!";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleAsync(int id)
        {
            var promo = await _context.Promotions.FindAsync(id);
            if (promo != null)
            {
                promo.IsActive = !promo.IsActive;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã {(promo.IsActive ? "kích hoạt" : "tắt")} chương trình '{promo.PromoName}'!";
            }
            return RedirectToPage();
        }
    }
}
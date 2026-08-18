using System.ComponentModel.DataAnnotations;
using BookStore.Data;
using BookStore.Models.Entities;
using BookStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Pages.Staff.Promotions
{
    [Authorize(Roles = "Staff,Admin")]
    public class CreateModel : PageModel
    {
        private readonly IStaffService _staffService;
        private readonly ApplicationDbContext _context;

        public CreateModel(IStaffService staffService, ApplicationDbContext context)
        {
            _staffService = staffService;
            _context = context;
        }

        [BindProperty]
        public PromotionInputModel Input { get; set; } = new();

        public List<Book> AvailableBooks { get; set; } = new();

        public class PromotionInputModel
        {
            [Required(ErrorMessage = "Tên chiến dịch là bắt buộc")]
            [StringLength(200)]
            [Display(Name = "Tên chiến dịch khuyến mãi")]
            public string Name { get; set; } = string.Empty;

            [StringLength(1000)]
            [Display(Name = "Mô tả")]
            public string? Description { get; set; }

            [Required]
            [Range(1, 99, ErrorMessage = "Phần trăm giảm từ 1% đến 99%")]
            [Display(Name = "Phần trăm giảm giá (%)")]
            public int DiscountPercent { get; set; } = 15;

            [Required]
            [DataType(DataType.Date)]
            [Display(Name = "Ngày bắt đầu")]
            public DateTime StartDate { get; set; } = DateTime.Today;

            [Required]
            [DataType(DataType.Date)]
            [Display(Name = "Ngày kết thúc")]
            public DateTime EndDate { get; set; } = DateTime.Today.AddDays(7);

            public List<int> SelectedBookIds { get; set; } = new();
        }

        public async Task OnGetAsync()
        {
            AvailableBooks = await _context.Books.Where(b => b.IsActive).OrderBy(b => b.Title).ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                AvailableBooks = await _context.Books.Where(b => b.IsActive).OrderBy(b => b.Title).ToListAsync();
                return Page();
            }

            var promo = new Promotion
            {
                PromoName = Input.Name,
                DiscountPercent = Input.DiscountPercent,
                StartDate = Input.StartDate,
                EndDate = Input.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                IsActive = true
            };

            await _staffService.CreatePromotionAsync(promo, Input.SelectedBookIds);
            TempData["SuccessMessage"] = $"Tạo chương trình khuyến mãi '{promo.PromoName}' thành công!";
            return RedirectToPage("/Staff/Promotions/Index");
        }
    }
}

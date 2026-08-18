using System.ComponentModel.DataAnnotations;
using BookStore.Models.Entities;
using BookStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Pages.Admin.Vouchers
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly IAdminService _adminService;

        public CreateModel(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [BindProperty]
        public VoucherInputModel Input { get; set; } = new();

        public class VoucherInputModel
        {
            [Required(ErrorMessage = "Mã voucher là bắt buộc")]
            [StringLength(50)]
            [Display(Name = "Mã Voucher (Code)")]
            public string Code { get; set; } = string.Empty;

            [Range(0, 100)]
            [Display(Name = "Phần trăm giảm (%)")]
            public int DiscountPercent { get; set; }

            [Display(Name = "Số tiền giảm cố định (VNĐ)")]
            public decimal DiscountAmount { get; set; }

            [Required]
            [Display(Name = "Đơn hàng tối thiểu (VNĐ)")]
            public decimal MinOrderValue { get; set; } = 100000;

            [Display(Name = "Số tiền giảm tối đa (VNĐ)")]
            public decimal? MaxDiscount { get; set; }

            [Required]
            [DataType(DataType.Date)]
            [Display(Name = "Ngày bắt đầu")]
            public DateTime StartDate { get; set; } = DateTime.Today;

            [Required]
            [DataType(DataType.Date)]
            [Display(Name = "Ngày kết thúc")]
            public DateTime EndDate { get; set; } = DateTime.Today.AddDays(30);

            [Required]
            [Range(1, 100000)]
            [Display(Name = "Tổng lượt sử dụng")]
            public int UsageLimit { get; set; } = 100;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var voucher = new Voucher
            {
                Code = Input.Code.Trim().ToUpper(),
                DiscountPercent = Input.DiscountPercent,
                DiscountAmount = Input.DiscountAmount,
                MinOrderValue = Input.MinOrderValue,
                MaxDiscount = Input.MaxDiscount,
                StartDate = Input.StartDate,
                EndDate = Input.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                UsageLimit = Input.UsageLimit,
                Status = 1
            };

            await _adminService.CreateVoucherAsync(voucher);
            TempData["SuccessMessage"] = $"Tạo voucher '{voucher.Code}' thành công!";
            return RedirectToPage("/Admin/Vouchers/Index");
        }
    }
}

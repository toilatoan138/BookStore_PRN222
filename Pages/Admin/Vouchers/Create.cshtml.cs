using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
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
            [StringLength(50, ErrorMessage = "Mã voucher không được vượt quá 50 ký tự")]
            [Display(Name = "Mã Voucher (Code)")]
            public string Code { get; set; } = string.Empty;

            [Range(0, 100, ErrorMessage = "Phần trăm giảm phải từ 0% đến 100%")]
            [Display(Name = "Phần trăm giảm (%)")]
            public int DiscountPercent { get; set; }

            [Range(0, 500000000, ErrorMessage = "Số tiền giảm tối đa không vượt quá 500.000.000 VNĐ")]
            [Display(Name = "Số tiền giảm cố định (VNĐ)")]
            public decimal DiscountAmount { get; set; }

            [Required(ErrorMessage = "Giá trị đơn tối thiểu là bắt buộc")]
            [Range(0, 500000000, ErrorMessage = "Giá trị đơn tối thiểu không hợp lệ")]
            [Display(Name = "Đơn hàng tối thiểu (VNĐ)")]
            public decimal MinOrderValue { get; set; } = 100000;

            [Range(0, 500000000, ErrorMessage = "Số tiền giảm tối đa không vượt quá 500.000.000 VNĐ")]
            [Display(Name = "Số tiền giảm tối đa (VNĐ)")]
            public decimal? MaxDiscount { get; set; }

            [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc")]
            [DataType(DataType.Date)]
            [Display(Name = "Ngày bắt đầu")]
            public DateTime StartDate { get; set; } = DateTime.Today;

            [Required(ErrorMessage = "Ngày kết thúc là bắt buộc")]
            [DataType(DataType.Date)]
            [Display(Name = "Ngày kết thúc")]
            public DateTime EndDate { get; set; } = DateTime.Today.AddDays(30);

            [Required(ErrorMessage = "Số lượt dùng tối đa là bắt buộc")]
            [Range(1, 100000, ErrorMessage = "Số lượt dùng phải từ 1 đến 100.000 lượt")]
            [Display(Name = "Tổng lượt sử dụng")]
            public int UsageLimit { get; set; } = 100;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // 1. Chặn lỗi tràn năm SqlDateTime (SQL Server yêu cầu năm từ 1753 trở lên)
            if (Input.StartDate.Year < 2020 || Input.EndDate.Year > 2099)
            {
                ModelState.AddModelError(string.Empty, "Năm áp dụng không hợp lệ (phải nằm trong khoảng 2020 - 2099).");
                return Page();
            }

            // 2. Chặn lỗi logic thời gian
            if (Input.EndDate.Date < Input.StartDate.Date)
            {
                ModelState.AddModelError("Input.EndDate", "Ngày kết thúc hiệu lực phải sau hoặc cùng ngày bắt đầu.");
                return Page();
            }

            // 3. Chặn trường hợp cả 2 hình thức giảm giá đều bằng 0
            if (Input.DiscountPercent <= 0 && Input.DiscountAmount <= 0)
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập Phần trăm giảm (%) hoặc Số tiền giảm cố định (VNĐ).");
                return Page();
            }

            var voucher = new Voucher
            {
                Code = Input.Code.Trim().ToUpper(),
                DiscountPercent = Input.DiscountPercent,
                DiscountAmount = Input.DiscountAmount,
                MinOrderValue = Input.MinOrderValue,
                MaxDiscount = Input.MaxDiscount,
                StartDate = Input.StartDate.Date,
                EndDate = Input.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                UsageLimit = Input.UsageLimit,
                Status = 1
            };

            try
            {
                await _adminService.CreateVoucherAsync(voucher);
                TempData["SuccessMessage"] = $"Tạo voucher '{voucher.Code}' thành công!";
                return RedirectToPage("/Admin/Vouchers/Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi lưu vào hệ thống: " + ex.Message);
                return Page();
            }
        }
    }
}
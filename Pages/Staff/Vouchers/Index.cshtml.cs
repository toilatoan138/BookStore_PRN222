using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookStore.Data;
using BookStore.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Pages.Staff.Vouchers
{
    [Authorize(Roles = "Staff,Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private const decimal MAX_MONEY_ALLOWED = 1_000_000_000m; // 1 tỷ VNĐ chống tràn số CSDL

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Voucher> Vouchers { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Keyword { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.Vouchers
                .Include(v => v.Orders)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                string kw = Keyword.Trim().ToLower();
                query = query.Where(v => v.Code.ToLower().Contains(kw));
            }

            Vouchers = await query
                .OrderByDescending(v => v.StartDate)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync(
            string code,
            decimal discountAmount,
            int discountPercent,
            decimal minOrderValue,
            decimal? maxDiscount,
            DateTime startDate,
            DateTime endDate,
            int usageLimit,
            int status)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                TempData["ErrorMessage"] = "Mã voucher không được để trống.";
                return RedirectToPage();
            }

            string upperCode = code.Trim().ToUpper();
            bool exists = await _context.Vouchers.AnyAsync(v => v.Code == upperCode);
            if (exists)
            {
                TempData["ErrorMessage"] = $"Mã khuyến mãi '{upperCode}' đã tồn tại trong hệ thống!";
                return RedirectToPage();
            }

            // Kiểm thực toàn diện Backend
            if (!ValidateVoucher(discountAmount, discountPercent, minOrderValue, ref maxDiscount, startDate, ref endDate, usageLimit, 0, out string error))
            {
                TempData["ErrorMessage"] = error;
                return RedirectToPage();
            }

            if (discountPercent > 0)
            {
                discountAmount = 0;
            }
            else
            {
                maxDiscount = null;
            }

            var voucher = new Voucher
            {
                Code = upperCode,
                DiscountAmount = discountAmount,
                DiscountPercent = discountPercent,
                MinOrderValue = minOrderValue,
                MaxDiscount = maxDiscount,
                StartDate = startDate.Date,
                EndDate = endDate,
                UsageLimit = usageLimit,
                Status = status
            };

            _context.Vouchers.Add(voucher);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã tạo mã Voucher '{voucher.Code}' thành công!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync(
            int voucherId,
            decimal discountAmount,
            int discountPercent,
            decimal minOrderValue,
            decimal? maxDiscount,
            DateTime startDate,
            DateTime endDate,
            int usageLimit,
            int status)
        {
            var voucher = await _context.Vouchers
                .Include(v => v.Orders)
                .FirstOrDefaultAsync(v => v.Id == voucherId);

            if (voucher == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy mã giảm giá cần cập nhật!";
                return RedirectToPage();
            }

            int usedCount = voucher.Orders?.Count ?? 0;

            // Kiểm thực toàn diện Backend
            if (!ValidateVoucher(discountAmount, discountPercent, minOrderValue, ref maxDiscount, startDate, ref endDate, usageLimit, usedCount, out string error))
            {
                TempData["ErrorMessage"] = error;
                return RedirectToPage();
            }

            if (discountPercent > 0)
            {
                discountAmount = 0;
            }
            else
            {
                maxDiscount = null;
            }

            voucher.DiscountAmount = discountAmount;
            voucher.DiscountPercent = discountPercent;
            voucher.MinOrderValue = minOrderValue;
            voucher.MaxDiscount = maxDiscount;
            voucher.StartDate = startDate.Date;
            voucher.EndDate = endDate;
            voucher.UsageLimit = usageLimit;
            voucher.Status = status;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã cập nhật thông tin Voucher '{voucher.Code}' thành công!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleAsync(int id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher != null)
            {
                voucher.Status = voucher.Status == 1 ? 0 : 1;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã {(voucher.Status == 1 ? "kích hoạt" : "tắt")} Voucher '{voucher.Code}'!";
            }
            return RedirectToPage();
        }

        private static bool ValidateVoucher(
            decimal amount,
            int percent,
            decimal minOrder,
            ref decimal? maxDiscount,
            DateTime start,
            ref DateTime end,
            int limit,
            int currentUsed,
            out string error)
        {
            error = string.Empty;

            // 1. Chặn số âm
            if (amount < 0 || percent < 0 || minOrder < 0 || (maxDiscount.HasValue && maxDiscount.Value < 0) || limit < 0)
            {
                error = "Các giá trị số tiền, phần trăm và số lượt dùng không được là số âm.";
                return false;
            }

            // 2. Chặn tràn ngưỡng lưu trữ Database
            if (amount > MAX_MONEY_ALLOWED || minOrder > MAX_MONEY_ALLOWED || (maxDiscount.HasValue && maxDiscount.Value > MAX_MONEY_ALLOWED))
            {
                error = "Số tiền nhập vào vượt quá giới hạn tối đa cho phép (1.000.000.000 VNĐ).";
                return false;
            }

            // 3. Kiểm tra logic loại giảm giá
            if (percent <= 0 && amount <= 0)
            {
                error = "Vui lòng nhập ít nhất một hình thức giảm giá (theo % hoặc số tiền cố định).";
                return false;
            }
            if (percent > 0 && amount > 0)
            {
                error = "Chỉ được chọn 1 trong 2 hình thức: Giảm theo % hoặc Giảm số tiền cố định.";
                return false;
            }
            if (percent > 100)
            {
                error = "Mức giảm theo phần trăm không được vượt quá 100%.";
                return false;
            }
            if (amount > 0 && minOrder > 0 && amount > minOrder)
            {
                error = $"Mức giảm cố định ({amount:N0}đ) không được lớn hơn Đơn tối thiểu ({minOrder:N0}đ).";
                return false;
            }

            // 4. Ngày bắt đầu phải sớm hơn ngày kết thúc
            if (start.Date >= end.Date)
            {
                error = "Ngày bắt đầu phải sớm hơn ngày kết thúc.";
                return false;
            }
            end = end.Date.AddDays(1).AddTicks(-1); // Tính đến 23:59:59 của ngày kết thúc

            // 5. Kiểm tra giới hạn lượt dùng
            if (currentUsed > 0 && limit > 0 && limit < currentUsed)
            {
                error = $"Giới hạn số lượt dùng không thể nhỏ hơn số lượt đã được sử dụng thực tế ({currentUsed} lượt).";
                return false;
            }

            if (maxDiscount.HasValue && maxDiscount.Value <= 0)
            {
                maxDiscount = null;
            }

            return true;
        }
    }
}
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

        public async Task<IActionResult> OnPostCreateAsync(string code, decimal discountAmount, int discountPercent, decimal minOrderValue, decimal? maxDiscount, DateTime startDate, DateTime endDate, int usageLimit, int status)
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
                TempData["ErrorMessage"] = $"Mã voucher '{upperCode}' đã tồn tại trong hệ thống!";
                return RedirectToPage();
            }

            var voucher = new Voucher
            {
                Code = upperCode,
                DiscountAmount = discountAmount,
                DiscountPercent = discountPercent,
                MinOrderValue = minOrderValue,
                MaxDiscount = maxDiscount,
                StartDate = startDate,
                EndDate = endDate,
                UsageLimit = usageLimit,
                Status = status
            };

            _context.Vouchers.Add(voucher);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã tạo mã Voucher '{voucher.Code}' thành công!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync(int voucherId, decimal discountAmount, int discountPercent, decimal minOrderValue, decimal? maxDiscount, DateTime startDate, DateTime endDate, int usageLimit, int status)
        {
            var voucher = await _context.Vouchers.FindAsync(voucherId);
            if (voucher != null)
            {
                voucher.DiscountAmount = discountAmount;
                voucher.DiscountPercent = discountPercent;
                voucher.MinOrderValue = minOrderValue;
                voucher.MaxDiscount = maxDiscount;
                voucher.StartDate = startDate;
                voucher.EndDate = endDate;
                voucher.UsageLimit = usageLimit;
                voucher.Status = status;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã cập nhật thông tin Voucher '{voucher.Code}'!";
            }
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

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher != null)
            {
                _context.Vouchers.Remove(voucher);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa mã voucher!";
            }
            return RedirectToPage();
        }
    }
}

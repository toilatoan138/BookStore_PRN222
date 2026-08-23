using BookStore.Data;
using BookStore.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Pages.Admin.Vouchers
{
    [Authorize(Roles = "Admin,Staff")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Voucher> Vouchers { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Lấy danh sách voucher từ bảng Vouchers
            Vouchers = await _context.Vouchers
                .AsNoTracking()
                .OrderByDescending(v => v.Id)
                .ToListAsync();
        }

        // Xử lý nút Kích hoạt / Tạm dừng Voucher
        public async Task<IActionResult> OnPostToggleStatusAsync(int id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy mã giảm giá!";
                return RedirectToPage();
            }

            // Chuyển đổi trạng thái (1 = Active <-> 0 = Tạm dừng)
            voucher.Status = (voucher.Status == 1) ? 0 : 1;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã cập nhật trạng thái cho mã '{voucher.Code}' thành công!";
            return RedirectToPage();
        }
    }
}
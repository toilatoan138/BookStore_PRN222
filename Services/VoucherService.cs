using BookStore.Data;
using BookStore.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Services
{
    public class VoucherService : IVoucherService
    {
        private readonly ApplicationDbContext _context;

        public VoucherService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Voucher>> GetActiveVouchersAsync()
        {
            return await _context.Vouchers
                .Where(v => v.Status == 1 && v.StartDate <= DateTime.UtcNow && v.EndDate >= DateTime.UtcNow)
                .OrderByDescending(v => v.DiscountPercent)
                .ThenByDescending(v => v.DiscountAmount)
                .ToListAsync();
        }

        public async Task<List<UserVoucher>> GetUserWalletVouchersAsync(string userId)
        {
            return await _context.UserVouchers
                .Include(uv => uv.Voucher)
                .Where(uv => uv.UserId == userId)
                .OrderBy(uv => uv.IsUsed)
                .ThenByDescending(uv => uv.SavedDate)
                .ToListAsync();
        }

        public async Task<(bool Success, string Message)> SaveVoucherToWalletAsync(string userId, int voucherId)
        {
            var voucher = await _context.Vouchers.FindAsync(voucherId);
            if (voucher == null || voucher.Status != 1 || voucher.EndDate < DateTime.UtcNow)
            {
                return (false, "Voucher không tồn tại hoặc đã hết hạn.");
            }

            var existing = await _context.UserVouchers
                .FirstOrDefaultAsync(uv => uv.UserId == userId && uv.VoucherId == voucherId);

            if (existing != null)
            {
                return (false, "Bạn đã lưu voucher này vào ví rồi.");
            }

            _context.UserVouchers.Add(new UserVoucher
            {
                UserId = userId,
                VoucherId = voucherId,
                IsUsed = false,
                SavedDate = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return (true, "Lưu voucher vào ví thành công!");
        }

        public async Task<Voucher?> GetVoucherByCodeAsync(string code)
        {
            string cleanCode = code.Trim().ToUpper();
            return await _context.Vouchers
                .FirstOrDefaultAsync(v => v.Code.ToUpper() == cleanCode && v.Status == 1 &&
                                          v.StartDate <= DateTime.UtcNow && v.EndDate >= DateTime.UtcNow);
        }
    }
}

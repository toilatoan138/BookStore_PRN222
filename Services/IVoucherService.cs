using BookStore.Models.Entities;

namespace BookStore.Services
{
    public interface IVoucherService
    {
        Task<List<Voucher>> GetActiveVouchersAsync();
        Task<List<UserVoucher>> GetUserWalletVouchersAsync(string userId);
        Task<(bool Success, string Message)> SaveVoucherToWalletAsync(string userId, int voucherId);
        Task<Voucher?> GetVoucherByCodeAsync(string code);
    }
}

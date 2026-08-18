using BookStore.Models.Entities;

namespace BookStore.Services
{
    public interface IUserService
    {
        Task<ApplicationUser?> GetUserByIdAsync(string userId);
        Task<ApplicationUser?> GetUserByEmailAsync(string email);
        Task<bool> UpdateProfileAsync(string userId, string fullName, string? phoneNumber);
        Task<List<Address>> GetAddressesByUserIdAsync(string userId);
        Task<Address?> GetAddressByIdAsync(int addressId, string userId);
        Task<Address> AddAddressAsync(Address address);
        Task<bool> UpdateAddressAsync(Address address);
        Task<bool> DeleteAddressAsync(int addressId, string userId);
        Task<bool> SetDefaultShippingAddressAsync(int addressId, string userId);
        Task<List<WalletHistory>> GetWalletHistoriesAsync(string userId);
        Task<bool> UpdateWalletBalanceAsync(string userId, decimal amount, string transactionType, string description, int? orderId = null);
        Task<List<FPointHistory>> GetFPointHistoriesAsync(string userId);
        Task<bool> AddFPointsAsync(string userId, int amount, string reason, string actionType = "add");
    }
}

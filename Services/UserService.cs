using BookStore.Data;
using BookStore.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(ApplicationDbContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
        {
            return await _context.Users
                .Include(u => u.Addresses)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> UpdateProfileAsync(string userId, string fullName, string? phoneNumber)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.FullName = fullName;
            user.PhoneNumber = phoneNumber;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Address>> GetAddressesByUserIdAsync(string userId)
        {
            return await _context.Addresses
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsDefaultShipping)
                .ThenByDescending(a => a.Id)
                .ToListAsync();
        }

        public async Task<Address?> GetAddressByIdAsync(int addressId, string userId)
        {
            return await _context.Addresses.FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);
        }

        public async Task<Address> AddAddressAsync(Address address)
        {
            var existingAddresses = await _context.Addresses.Where(a => a.UserId == address.UserId).ToListAsync();
            if (!existingAddresses.Any())
            {
                address.IsDefaultShipping = true;
                address.IsDefaultBilling = true;
            }
            else if (address.IsDefaultShipping)
            {
                foreach (var addr in existingAddresses)
                {
                    addr.IsDefaultShipping = false;
                }
            }

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();
            return address;
        }

        public async Task<bool> UpdateAddressAsync(Address address)
        {
            var existing = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == address.Id && a.UserId == address.UserId);
            if (existing == null) return false;

            if (address.IsDefaultShipping)
            {
                var others = await _context.Addresses.Where(a => a.UserId == address.UserId && a.Id != address.Id).ToListAsync();
                foreach (var o in others) o.IsDefaultShipping = false;
            }

            existing.FullName = address.FullName;
            existing.Phone = address.Phone;
            existing.City = address.City;
            existing.District = address.District;
            existing.Ward = address.Ward;
            existing.AddressDetail = address.AddressDetail;
            existing.IsDefaultShipping = address.IsDefaultShipping;
            existing.IsDefaultBilling = address.IsDefaultBilling;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAddressAsync(int addressId, string userId)
        {
            var addr = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);
            if (addr == null) return false;

            _context.Addresses.Remove(addr);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetDefaultShippingAddressAsync(int addressId, string userId)
        {
            var addresses = await _context.Addresses.Where(a => a.UserId == userId).ToListAsync();
            var target = addresses.FirstOrDefault(a => a.Id == addressId);
            if (target == null) return false;

            foreach (var a in addresses)
            {
                a.IsDefaultShipping = (a.Id == addressId);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<WalletHistory>> GetWalletHistoriesAsync(string userId)
        {
            return await _context.WalletHistories
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> UpdateWalletBalanceAsync(string userId, decimal amount, string transactionType, string description, int? orderId = null)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            if (amount < 0 && user.WalletBalance + amount < 0)
            {
                return false; // Insufficient balance
            }

            user.WalletBalance += amount;

            var history = new WalletHistory
            {
                UserId = userId,
                Amount = amount,
                TransactionType = transactionType,
                Description = description,
                OrderId = orderId,
                CreatedAt = DateTime.UtcNow
            };

            _context.WalletHistories.Add(history);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<FPointHistory>> GetFPointHistoriesAsync(string userId)
        {
            return await _context.FPointHistories
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> AddFPointsAsync(string userId, int amount, string reason, string actionType = "add")
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            if (actionType == "add")
            {
                user.FPoints += amount;
            }
            else if (actionType == "sub")
            {
                user.FPoints = Math.Max(0, user.FPoints - amount);
            }

            var history = new FPointHistory
            {
                UserId = userId,
                Amount = amount,
                ActionType = actionType,
                Reason = reason,
                CustomerInfo = user.FullName,
                CreatedAt = DateTime.UtcNow
            };

            _context.FPointHistories.Add(history);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

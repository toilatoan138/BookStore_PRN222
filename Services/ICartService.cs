using BookStore.Models.Entities;

namespace BookStore.Services
{
    public interface ICartService
    {
        Task<List<CartItem>> GetCartItemsAsync(string userId);
        Task<int> GetCartCountAsync(string userId);
        Task<bool> AddToCartAsync(string userId, int bookId, int quantity = 1);
        Task<bool> UpdateQuantityAsync(string userId, int bookId, int quantity);
        Task<bool> RemoveFromCartAsync(string userId, int bookId);
        Task<bool> ClearCartAsync(string userId);
        Task<bool> RemovePurchasedItemsAsync(string userId, IEnumerable<int> purchasedBookIds);
    }
}

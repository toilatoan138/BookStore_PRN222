using BookStore.Data;
using BookStore.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;

        public CartService(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task<Cart> GetOrCreateCartAsync(string userId)
        {
            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }
            return cart;
        }

        public async Task<List<CartItem>> GetCartItemsAsync(string userId)
        {
            return await _context.CartItems
                .Include(ci => ci.Book)
                    .ThenInclude(b => b.Category)
                .Where(ci => ci.Cart.UserId == userId)
                .ToListAsync();
        }

        public async Task<int> GetCartCountAsync(string userId)
        {
            int? count = await _context.CartItems
                .Where(ci => ci.Cart.UserId == userId)
                .SumAsync(ci => (int?)ci.Quantity);

            return count ?? 0;
        }

        public async Task<bool> AddToCartAsync(string userId, int bookId, int quantity = 1)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null || !book.IsActive || book.StockQuantity <= 0) return false;

            var cart = await GetOrCreateCartAsync(userId);

            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cart.CartId && ci.BookId == bookId);

            if (existingItem != null)
            {
                existingItem.Quantity = Math.Min(existingItem.Quantity + quantity, book.StockQuantity);
            }
            else
            {
                var newItem = new CartItem
                {
                    CartId = cart.CartId,
                    BookId = bookId,
                    Quantity = Math.Min(quantity, book.StockQuantity),
                    CreatedAt = DateTime.UtcNow
                };
                _context.CartItems.Add(newItem);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateQuantityAsync(string userId, int bookId, int quantity)
        {
            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null) return false;

            var item = await _context.CartItems
                .Include(ci => ci.Book)
                .FirstOrDefaultAsync(ci => ci.CartId == cart.CartId && ci.BookId == bookId);

            if (item == null) return false;

            if (quantity <= 0)
            {
                _context.CartItems.Remove(item);
            }
            else
            {
                item.Quantity = Math.Min(quantity, item.Book.StockQuantity);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveFromCartAsync(string userId, int bookId)
        {
            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null) return false;

            var item = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cart.CartId && ci.BookId == bookId);

            if (item == null) return false;

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ClearCartAsync(string userId)
        {
            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null) return true;

            var items = await _context.CartItems.Where(ci => ci.CartId == cart.CartId).ToListAsync();
            if (items.Any())
            {
                _context.CartItems.RemoveRange(items);
                await _context.SaveChangesAsync();
            }
            return true;
        }

        public async Task<bool> RemovePurchasedItemsAsync(string userId, IEnumerable<int> purchasedBookIds)
        {
            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null) return true;

            var items = await _context.CartItems
                .Where(ci => ci.CartId == cart.CartId && purchasedBookIds.Contains(ci.BookId))
                .ToListAsync();

            if (items.Any())
            {
                _context.CartItems.RemoveRange(items);
                await _context.SaveChangesAsync();
            }
            return true;
        }
    }
}

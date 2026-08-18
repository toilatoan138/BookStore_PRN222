using BookStore.Data;
using BookStore.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Services
{
    public class CollectionService : ICollectionService
    {
        private readonly ApplicationDbContext _context;

        public CollectionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Collection>> GetUserCollectionsAsync(string userId)
        {
            return await _context.Collections
                .Include(c => c.CollectionBooks)
                    .ThenInclude(cb => cb.Book)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Collection?> GetCollectionByIdAsync(int collectionId, string? userId = null)
        {
            var query = _context.Collections
                .Include(c => c.CollectionBooks)
                    .ThenInclude(cb => cb.Book)
                .Include(c => c.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(c => c.Id == collectionId && (c.UserId == userId || c.IsPublic));
            }
            else
            {
                query = query.Where(c => c.Id == collectionId && c.IsPublic);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<Collection> CreateCollectionAsync(string userId, string name, string? description, bool isPublic, string? coverColor)
        {
            var collection = new Collection
            {
                UserId = userId,
                Name = name,
                Description = description,
                IsPublic = isPublic,
                CoverColor = coverColor ?? "#C92127",
                CreatedAt = DateTime.UtcNow
            };

            _context.Collections.Add(collection);
            await _context.SaveChangesAsync();
            return collection;
        }

        public async Task<bool> UpdateCollectionAsync(int collectionId, string userId, string name, string? description, bool isPublic, string? coverColor)
        {
            var col = await _context.Collections.FirstOrDefaultAsync(c => c.Id == collectionId && c.UserId == userId);
            if (col == null) return false;

            col.Name = name;
            col.Description = description;
            col.IsPublic = isPublic;
            if (!string.IsNullOrEmpty(coverColor)) col.CoverColor = coverColor;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCollectionAsync(int collectionId, string userId)
        {
            var col = await _context.Collections.FirstOrDefaultAsync(c => c.Id == collectionId && c.UserId == userId);
            if (col == null) return false;

            _context.Collections.Remove(col);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddBookToCollectionAsync(int collectionId, int bookId, string userId)
        {
            var col = await _context.Collections.FirstOrDefaultAsync(c => c.Id == collectionId && c.UserId == userId);
            if (col == null) return false;

            bool exists = await _context.CollectionBooks.AnyAsync(cb => cb.CollectionId == collectionId && cb.BookId == bookId);
            if (exists) return true;

            _context.CollectionBooks.Add(new CollectionBook
            {
                CollectionId = collectionId,
                BookId = bookId,
                AddedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveBookFromCollectionAsync(int collectionId, int bookId, string userId)
        {
            var col = await _context.Collections.FirstOrDefaultAsync(c => c.Id == collectionId && c.UserId == userId);
            if (col == null) return false;

            var item = await _context.CollectionBooks.FirstOrDefaultAsync(cb => cb.CollectionId == collectionId && cb.BookId == bookId);
            if (item == null) return false;

            _context.CollectionBooks.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

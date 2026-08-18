using BookStore.Data;
using BookStore.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Services
{
    public class BookService : IBookService
    {
        private readonly ApplicationDbContext _context;

        public BookService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<Book>> GetBooksPagedAsync(BookFilterParams filter)
        {
            var query = _context.Books
                .Include(b => b.Category)
                .Where(b => b.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                string kw = filter.Keyword.Trim().ToLower();
                query = query.Where(b => (b.Title != null && b.Title.ToLower().Contains(kw)) ||
                                         (b.Author != null && b.Author.ToLower().Contains(kw)) ||
                                         (b.Publisher != null && b.Publisher.ToLower().Contains(kw)));
            }

            if (filter.CategoryId.HasValue && filter.CategoryId.Value > 0)
            {
                query = query.Where(b => b.CategoryId == filter.CategoryId.Value ||
                                         (b.Category != null && b.Category.ParentId == filter.CategoryId.Value));
            }

            if (filter.MinPrice.HasValue)
            {
                query = query.Where(b => b.Price >= filter.MinPrice.Value);
            }

            if (filter.MaxPrice.HasValue)
            {
                query = query.Where(b => b.Price <= filter.MaxPrice.Value);
            }

            query = filter.SortBy switch
            {
                "price_asc" => query.OrderBy(b => b.Price),
                "price_desc" => query.OrderByDescending(b => b.Price),
                "bestseller" => query.OrderByDescending(b => b.SoldQuantity),
                "az" => query.OrderBy(b => b.Title),
                _ => query.OrderByDescending(b => b.Id) // Default: newest
            };

            int totalItems = await query.CountAsync();
            int pageIndex = filter.PageIndex < 1 ? 1 : filter.PageIndex;
            int pageSize = filter.PageSize < 1 ? 12 : filter.PageSize;

            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Book>
            {
                Items = items,
                TotalItems = totalItems,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
        }

        public async Task<Book?> GetBookByIdAsync(int id)
        {
            return await _context.Books
                .Include(b => b.Category)
                .Include(b => b.DetailImages)
                .Include(b => b.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<List<Book>> GetTopSellingBooksAsync(int limit = 10)
        {
            return await _context.Books
                .Include(b => b.Category)
                .Where(b => b.IsActive)
                .OrderByDescending(b => b.SoldQuantity)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<Book>> GetNewArrivalsAsync(int limit = 10)
        {
            return await _context.Books
                .Include(b => b.Category)
                .Where(b => b.IsActive)
                .OrderByDescending(b => b.Id)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<Book>> GetFlashSaleBooksAsync(int limit = 10)
        {
            var now = DateTime.UtcNow;
            var promoBookIds = await _context.PromotionBooks
                .Where(pb => pb.Promotion.IsActive && pb.Promotion.StartDate <= now && pb.Promotion.EndDate >= now)
                .Select(pb => pb.BookId)
                .Distinct()
                .ToListAsync();

            if (promoBookIds.Any())
            {
                return await _context.Books
                    .Include(b => b.Category)
                    .Where(b => b.IsActive && promoBookIds.Contains(b.Id) && b.StockQuantity > 0)
                    .OrderByDescending(b => b.SoldQuantity)
                    .Take(limit)
                    .ToListAsync();
            }

            return await _context.Books
                .Include(b => b.Category)
                .Where(b => b.IsActive && b.StockQuantity > 0)
                .OrderByDescending(b => b.SoldQuantity)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<Book>> GetBooksByKeywordAsync(string keyword, int limit = 12)
        {
            string kw = keyword.Trim().ToLower();
            return await _context.Books
                .Include(b => b.Category)
                .Where(b => b.IsActive && (b.Title.ToLower().Contains(kw) || (b.Author != null && b.Author.ToLower().Contains(kw))))
                .OrderByDescending(b => b.SoldQuantity)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<Book>> GetSuggestedBooksAsync(int currentBookId, int categoryId, int limit = 6)
        {
            return await _context.Books
                .Where(b => b.IsActive && b.Id != currentBookId && b.CategoryId == categoryId)
                .OrderByDescending(b => b.SoldQuantity)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await _context.Categories
                .Include(c => c.Children)
                .Include(c => c.Books)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<bool> AddReviewAsync(string userId, int bookId, int rating, string comment)
        {
            var review = new Review
            {
                UserId = userId,
                BookId = bookId,
                Rating = Math.Clamp(rating, 1, 5),
                Comment = comment,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Review>> GetBookReviewsAsync(int bookId)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.BookId == bookId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
    }
}

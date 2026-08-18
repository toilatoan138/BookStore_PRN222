using BookStore.Models.Entities;

namespace BookStore.Services
{
    public class BookFilterParams
    {
        public string? Keyword { get; set; }
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? SortBy { get; set; } // "price_asc", "price_desc", "newest", "bestseller", "az"
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalItems { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / (PageSize > 0 ? PageSize : 1));
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
    }

    public interface IBookService
    {
        Task<PagedResult<Book>> GetBooksPagedAsync(BookFilterParams filter);
        Task<Book?> GetBookByIdAsync(int id);
        Task<List<Book>> GetTopSellingBooksAsync(int limit = 10);
        Task<List<Book>> GetNewArrivalsAsync(int limit = 10);
        Task<List<Book>> GetFlashSaleBooksAsync(int limit = 10);
        Task<List<Book>> GetBooksByKeywordAsync(string keyword, int limit = 12);
        Task<List<Book>> GetSuggestedBooksAsync(int currentBookId, int categoryId, int limit = 6);
        Task<List<Category>> GetCategoriesAsync();
        Task<bool> AddReviewAsync(string userId, int bookId, int rating, string comment);
        Task<List<Review>> GetBookReviewsAsync(int bookId);
    }
}

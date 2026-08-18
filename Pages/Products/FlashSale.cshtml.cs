using BookStore.Data;
using BookStore.Models.Entities;
using BookStore.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Pages.Products
{
    public class FlashSaleModel : PageModel
    {
        private readonly IBookService _bookService;
        private readonly ApplicationDbContext _context;

        public FlashSaleModel(IBookService bookService, ApplicationDbContext context)
        {
            _bookService = bookService;
            _context = context;
        }

        public List<Book> FlashBooks { get; set; } = new();
        public Promotion? ActivePromotion { get; set; }

        public async Task OnGetAsync()
        {
            var now = DateTime.UtcNow;
            ActivePromotion = await _context.Promotions
                .Where(p => p.IsActive && p.StartDate <= now && p.EndDate >= now)
                .OrderByDescending(p => p.DiscountPercent)
                .FirstOrDefaultAsync();

            FlashBooks = await _bookService.GetFlashSaleBooksAsync(24);
        }
    }
}

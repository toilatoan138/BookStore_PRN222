using BookStore.Data;
using BookStore.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Pages.Admin.Products
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Book> Books { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Keyword { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        public List<Category> Categories { get; set; } = new();

        public async Task OnGetAsync()
        {
            Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();

            var query = _context.Books
                .Include(b => b.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                string kw = Keyword.Trim().ToLower();
                query = query.Where(b => b.Title.ToLower().Contains(kw) || (b.Author != null && b.Author.ToLower().Contains(kw)));
            }

            if (CategoryId.HasValue && CategoryId.Value > 0)
            {
                query = query.Where(b => b.CategoryId == CategoryId.Value);
            }

            Books = await query.OrderByDescending(b => b.Id).ToListAsync();
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            book.IsActive = !book.IsActive;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã {(book.IsActive ? "kích hoạt hiển thị" : "ẩn")} sản phẩm '{book.Title}'!";
            return RedirectToPage(new { keyword = Keyword, categoryId = CategoryId });
        }
    }
}

using BookStore.Data;
using BookStore.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Pages.Staff.Promotions
{
    [Authorize(Roles = "Staff,Admin")]
    public class BooksModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public BooksModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Promotion? PromotionInfo { get; set; }
        public List<Book> AvailableBooks { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            PromotionInfo = await _context.Promotions
                .Include(p => p.PromotionBooks)
                    .ThenInclude(pb => pb.Book)
                        .ThenInclude(b => b.Category)
                .FirstOrDefaultAsync(p => p.PromoId == Id);

            if (PromotionInfo == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy chương trình khuyến mãi!";
                return RedirectToPage("/Staff/Promotions/Index");
            }

            var promoBookIds = PromotionInfo.PromotionBooks.Select(pb => pb.BookId).ToList();

            AvailableBooks = await _context.Books
                .Where(b => b.IsActive && !promoBookIds.Contains(b.Id))
                .OrderBy(b => b.Title)
                .Take(100)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAddBookAsync(int promoId, int bookId)
        {
            bool exists = await _context.PromotionBooks.AnyAsync(pb => pb.PromoId == promoId && pb.BookId == bookId);
            if (!exists)
            {
                _context.PromotionBooks.Add(new PromotionBook
                {
                    PromoId = promoId,
                    BookId = bookId
                });
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã thêm sách vào đợt Flash Sale!";
            }
            return RedirectToPage(new { id = promoId });
        }

        public async Task<IActionResult> OnPostRemoveBookAsync(int promoId, int bookId)
        {
            var item = await _context.PromotionBooks.FirstOrDefaultAsync(pb => pb.PromoId == promoId && pb.BookId == bookId);
            if (item != null)
            {
                _context.PromotionBooks.Remove(item);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa sách khỏi đợt Flash Sale!";
            }
            return RedirectToPage(new { id = promoId });
        }
    }
}

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
            var promo = await _context.Promotions.FindAsync(promoId);

            // Test 7: Ngăn chặn thêm sách vào Flash Sale đã tắt hoặc hết hạn
            if (promo == null || !promo.IsActive || promo.EndDate < DateTime.UtcNow)
            {
                TempData["ErrorMessage"] = "Không thể thêm sách. Đợt Flash Sale này đã bị tắt hoặc đã hết hạn!";
                return RedirectToPage(new { id = promoId });
            }

            var book = await _context.Books.FindAsync(bookId);

            // Test 5: Ngăn chặn thêm sách có số tồn kho bằng 0
            if (book == null || book.StockQuantity <= 0)
            {
                TempData["ErrorMessage"] = "Không thể thêm! Sách này hiện đang hết hàng tồn kho.";
                return RedirectToPage(new { id = promoId });
            }

            // Test 6: Chặn Double-Click / Lỗi trùng lặp dữ liệu vào cùng 1 bảng
            bool exists = await _context.PromotionBooks.AnyAsync(pb => pb.PromoId == promoId && pb.BookId == bookId);
            if (exists)
            {
                TempData["ErrorMessage"] = "Sách này đã tồn tại trong đợt khuyến mãi!";
                return RedirectToPage(new { id = promoId });
            }

            // Test 2: Chặn xung đột, trùng lặp khung giờ (Time Overlap Constraint)
            // Kiểm tra xem cuốn sách này có đang chạy ở 1 đợt Flash Sale Khác (đang Active) 
            // và thời gian có đè lên nhau không.
            bool hasOverlap = await _context.PromotionBooks
                .Include(pb => pb.Promotion)
                .AnyAsync(pb => pb.BookId == bookId
                             && pb.PromoId != promoId
                             && pb.Promotion.IsActive
                             && pb.Promotion.StartDate < promo.EndDate
                             && pb.Promotion.EndDate > promo.StartDate);

            if (hasOverlap)
            {
                TempData["ErrorMessage"] = "Lỗi xung đột! Sách này đang được áp dụng trong một đợt Flash Sale khác có trùng lặp khung giờ.";
                return RedirectToPage(new { id = promoId });
            }

            // Nếu vượt qua tất cả, tiến hành thêm
            _context.PromotionBooks.Add(new PromotionBook
            {
                PromoId = promoId,
                BookId = bookId
            });
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã thêm sách vào đợt Flash Sale thành công!";

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
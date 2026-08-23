using BookStore.Data;
using BookStore.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Pages.Admin.Products
{
    [Authorize(Roles = "Admin,Staff")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Book> Books { get; set; } = new();

        // Danh sách Categories dùng cho Dropdown Lọc dữ liệu
        public List<Category> Categories { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Keyword { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        // Biến Phân Trang
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 10;

        public async Task OnGetAsync()
        {
            // 1. Tải danh sách Danh mục cho thanh Filter
            Categories = await _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();

            // 2. Xây dựng truy vấn Sách cơ bản (Join với Category và Location)
            var query = _context.Books
                .Include(b => b.Category)
                .Include(b => b.Location)
                .AsNoTracking()
                .AsQueryable();

            // 3. Logic Tìm kiếm thông minh (Smart Search)
            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                string kw = Keyword.Trim().ToLower();
                query = query.Where(b =>
                    b.Title.ToLower().Contains(kw) ||
                    b.Author.ToLower().Contains(kw) ||
                    b.Isbn.ToLower() == kw ||
                    b.Id.ToString() == kw);
            }

            // 4. Logic Lọc theo Danh mục
            if (CategoryId.HasValue && CategoryId.Value > 0)
            {
                query = query.Where(b => b.CategoryId == CategoryId.Value);
            }

            // 5. Logic Phân trang Toán học
            int totalCount = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

            if (CurrentPage < 1) CurrentPage = 1;
            if (CurrentPage > TotalPages && TotalPages > 0) CurrentPage = TotalPages;

            // 6. Truy xuất dữ liệu cuối cùng (Sắp xếp mới nhất lên đầu)
            Books = await query
                .OrderByDescending(b => b.Id)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }

        // Logic Xóa Sách an toàn (Có kiểm tra dữ liệu liên quan)
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var book = await _context.Books
                .Include(b => b.OrderDetails)
                .Include(b => b.InventoryHistories)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy cuốn sách này!";
                return RedirectToPage(new { Keyword, CategoryId, CurrentPage });
            }

            // Bảo vệ dữ liệu: Không xóa sách đã có giao dịch mua bán hoặc nhập kho
            if (book.OrderDetails.Any() || book.InventoryHistories.Any())
            {
                TempData["ErrorMessage"] = $"Không thể xóa: Cuốn sách '{book.Title}' đã phát sinh giao dịch đơn hàng hoặc lịch sử kho. Vui lòng chuyển trạng thái sang 'Ngừng kinh doanh' thay vì xóa.";
                return RedirectToPage(new { Keyword, CategoryId, CurrentPage });
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã xóa cuốn sách '{book.Title}' thành công!";
            return RedirectToPage(new { Keyword, CategoryId, CurrentPage });
        }
    }
}
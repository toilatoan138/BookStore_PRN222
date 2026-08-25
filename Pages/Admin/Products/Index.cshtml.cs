using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public List<Category> Categories { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Keyword { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 10;

        public async Task OnGetAsync()
        {
            // 1. Tải danh mục cho bộ lọc
            Categories = await _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();

            // 2. Xây dựng truy vấn danh sách sách
            var query = _context.Books
                .Include(b => b.Category)
                .Include(b => b.Location)
                .AsNoTracking()
                .AsQueryable();

            // 3. Tìm kiếm từ khóa
            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                string kw = Keyword.Trim().ToLower();
                query = query.Where(b =>
                    (b.Title != null && b.Title.ToLower().Contains(kw)) ||
                    (b.Author != null && b.Author.ToLower().Contains(kw)) ||
                    (b.Isbn != null && b.Isbn.ToLower() == kw) ||
                    b.Id.ToString() == kw);
            }

            // 4. Lọc theo danh mục
            if (CategoryId.HasValue && CategoryId.Value > 0)
            {
                query = query.Where(b => b.CategoryId == CategoryId.Value);
            }

            // 5. Phân trang
            int totalCount = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

            if (CurrentPage < 1) CurrentPage = 1;
            if (CurrentPage > TotalPages && TotalPages > 0) CurrentPage = TotalPages;

            // 6. Lấy dữ liệu
            Books = await query
                .OrderByDescending(b => b.Id)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }

        // Handler Bật / Tắt trạng thái Ẩn - Hiện sách (Soft Delete)
        public async Task<IActionResult> OnPostToggleStatusAsync(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy cuốn sách yêu cầu!";
                return RedirectToPage(new { Keyword, CategoryId, CurrentPage });
            }

            book.IsActive = !book.IsActive;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = book.IsActive
                ? $"Đã mở bán lại cuốn sách '{book.Title}' trên cửa hàng!"
                : $"Đã ẩn cuốn sách '{book.Title}' khỏi cửa hàng!";

            return RedirectToPage(new { Keyword, CategoryId, CurrentPage });
        }

        // Handler Xóa Vĩnh Viễn Sách (Hard Delete có kiểm tra ràng buộc)
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

            // Bảo vệ dữ liệu: Chặn xóa sách đã phát sinh đơn hàng hoặc lịch sử kho
            if ((book.OrderDetails != null && book.OrderDetails.Any()) ||
                (book.InventoryHistories != null && book.InventoryHistories.Any()))
            {
                TempData["ErrorMessage"] = $"Không thể xóa vĩnh viễn cuốn sách '{book.Title}' vì đã phát sinh giao dịch đơn hàng hoặc lịch sử kho. Vui lòng sử dụng tính năng ẩn sách!";
                return RedirectToPage(new { Keyword, CategoryId, CurrentPage });
            }

            try
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Đã xóa vĩnh viễn cuốn sách '{book.Title}' thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Không thể xóa sách do vướng ràng buộc dữ liệu khác: " + ex.Message;
            }

            return RedirectToPage(new { Keyword, CategoryId, CurrentPage });
        }
    }
}
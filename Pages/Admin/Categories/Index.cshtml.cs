using BookStore.Data;
using BookStore.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Pages.Admin.Categories
{
    [Authorize(Roles = "Admin,Staff")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Category> Categories { get; set; } = new();

        // ĐÂY LÀ THUỘC TÍNH BỊ THIẾU GÂY RA LỖI CS1061
        [BindProperty]
        public Category Input { get; set; } = new();

        public async Task OnGetAsync()
        {
            Categories = await _context.Categories
                .Include(c => c.Parent)
                .Include(c => c.Books)
                .AsNoTracking()
                .OrderBy(c => c.ParentId).ThenBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ thông tin.";
                return RedirectToPage();
            }

            if (Input.ParentId == 0) Input.ParentId = null;

            _context.Categories.Add(Input);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã thêm danh mục mới thành công!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            var categoryToUpdate = await _context.Categories.FindAsync(Input.Id);
            if (categoryToUpdate == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy danh mục để sửa!";
                return RedirectToPage();
            }

            categoryToUpdate.Name = Input.Name;
            categoryToUpdate.Description = Input.Description;
            categoryToUpdate.ImageUrl = Input.ImageUrl;

            if (Input.ParentId == 0) categoryToUpdate.ParentId = null;
            else categoryToUpdate.ParentId = Input.ParentId;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã cập nhật danh mục thành công!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Children)
                .Include(c => c.Books)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return RedirectToPage();

            if (category.Children.Any() || category.Books.Any())
            {
                TempData["ErrorMessage"] = $"Không thể xóa: '{category.Name}' đang chứa sách hoặc danh mục con!";
                return RedirectToPage();
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa danh mục thành công!";
            return RedirectToPage();
        }
    }
}
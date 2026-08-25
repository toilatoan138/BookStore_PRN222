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

        [BindProperty]
        public Category Input { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchKeyword { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.Categories
                .Include(c => c.Parent)
                .Include(c => c.Books)
                .AsNoTracking()
                .AsQueryable();

            // Tích hợp Tìm kiếm
            if (!string.IsNullOrWhiteSpace(SearchKeyword))
            {
                string kw = SearchKeyword.Trim().ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(kw) || c.Id.ToString() == kw);
            }

            // Sắp xếp danh mục Gốc lên trước, sau đó đến Tên
            Categories = await query
                .OrderBy(c => c.ParentId)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (string.IsNullOrWhiteSpace(Input.Name))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập tên danh mục hợp lệ.";
                return RedirectToPage();
            }

            if (Input.ParentId == 0) Input.ParentId = null;

            _context.Categories.Add(new Category
            {
                Name = Input.Name.Trim(),
                Description = Input.Description?.Trim(),
                ParentId = Input.ParentId
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã thêm danh mục mới thành công!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            if (string.IsNullOrWhiteSpace(Input.Name))
            {
                TempData["ErrorMessage"] = "Tên danh mục không được để trống!";
                return RedirectToPage();
            }

            var categoryToUpdate = await _context.Categories.FindAsync(Input.Id);
            if (categoryToUpdate == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy danh mục để sửa!";
                return RedirectToPage();
            }

            // Phòng chống lỗi chọn cha là chính nó (Circular Reference)
            if (Input.ParentId == Input.Id)
            {
                TempData["ErrorMessage"] = "Một danh mục không thể làm cha của chính nó!";
                return RedirectToPage();
            }

            categoryToUpdate.Name = Input.Name.Trim();
            categoryToUpdate.Description = Input.Description?.Trim();
            categoryToUpdate.ParentId = (Input.ParentId == 0) ? null : Input.ParentId;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã cập nhật danh mục '{categoryToUpdate.Name}' thành công!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Children)
                .Include(c => c.Books)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return RedirectToPage();

            // Khóa chốt an toàn CSDL
            if (category.Children.Any() || category.Books.Any())
            {
                TempData["ErrorMessage"] = $"Không thể xóa: Danh mục '{category.Name}' đang chứa sách hoặc danh mục con bên trong!";
                return RedirectToPage();
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa danh mục thành công!";
            return RedirectToPage();
        }
    }
}
using System.ComponentModel.DataAnnotations;
using BookStore.Models.Entities;
using BookStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Pages.Admin.Categories
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ICategoryService _categoryService;

        public IndexModel(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public List<Category> Categories { get; set; } = new();

        [BindProperty]
        public CategoryInputModel Input { get; set; } = new();

        public class CategoryInputModel
        {
            public int? Id { get; set; }

            [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
            [StringLength(100)]
            [Display(Name = "Tên danh mục")]
            public string Name { get; set; } = string.Empty;

            [StringLength(500)]
            [Display(Name = "Mô tả")]
            public string? Description { get; set; }

            [StringLength(500)]
            [Display(Name = "URL Ảnh minh họa")]
            public string? ImageUrl { get; set; }

            [Display(Name = "Danh mục cha")]
            public int? ParentId { get; set; }
        }

        public async Task OnGetAsync()
        {
            Categories = await _categoryService.GetAllCategoriesAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                Categories = await _categoryService.GetAllCategoriesAsync();
                return Page();
            }

            var cat = new Category
            {
                Name = Input.Name,
                Description = Input.Description,
                ImageUrl = Input.ImageUrl,
                ParentId = Input.ParentId > 0 ? Input.ParentId : null
            };

            await _categoryService.CreateCategoryAsync(cat);
            TempData["SuccessMessage"] = $"Thêm danh mục '{cat.Name}' thành công!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            if (!Input.Id.HasValue || Input.Id <= 0)
            {
                TempData["ErrorMessage"] = "Không tìm thấy danh mục cần sửa.";
                return RedirectToPage();
            }

            var cat = new Category
            {
                Id = Input.Id.Value,
                Name = Input.Name,
                Description = Input.Description,
                ImageUrl = Input.ImageUrl,
                ParentId = Input.ParentId > 0 ? Input.ParentId : null
            };

            await _categoryService.UpdateCategoryAsync(cat);
            TempData["SuccessMessage"] = "Cập nhật danh mục thành công!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            bool success = await _categoryService.DeleteCategoryAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = "Đã xóa danh mục thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể xóa danh mục này vì đang chứa sách hoặc danh mục con.";
            }

            return RedirectToPage();
        }
    }
}

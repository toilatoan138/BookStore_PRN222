using System.ComponentModel.DataAnnotations;
using BookStore.Data;
using BookStore.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Pages.Admin.Products
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public BookInputModel Input { get; set; } = new();

        public List<Category> Categories { get; set; } = new();

        public class BookInputModel
        {
            [Required(ErrorMessage = "Tiêu đề sách là bắt buộc")]
            [StringLength(300)]
            [Display(Name = "Tiêu đề")]
            public string Title { get; set; } = string.Empty;

            [StringLength(200)]
            [Display(Name = "Tác giả")]
            public string? Author { get; set; }

            [Required(ErrorMessage = "Vui lòng chọn danh mục")]
            [Display(Name = "Danh mục")]
            public int CategoryId { get; set; }

            [Required(ErrorMessage = "Giá bán là bắt buộc")]
            [Range(0, 100000000, ErrorMessage = "Giá bán phải từ 0đ trở lên")]
            [Display(Name = "Giá bán (VNĐ)")]
            public decimal Price { get; set; }

            [Display(Name = "Giá nhập (VNĐ)")]
            public decimal ImportPrice { get; set; }

            [Required(ErrorMessage = "Số lượng tồn kho là bắt buộc")]
            [Range(0, 100000, ErrorMessage = "Số lượng tồn kho phải >= 0")]
            [Display(Name = "Số lượng tồn kho")]
            public int StockQuantity { get; set; } = 10;

            [StringLength(500)]
            [Display(Name = "URL Ảnh bìa chính")]
            public string? ImageUrl { get; set; }

            [StringLength(200)]
            [Display(Name = "Nhà xuất bản")]
            public string? Publisher { get; set; }

            [StringLength(200)]
            [Display(Name = "Nhà cung cấp")]
            public string? Supplier { get; set; }

            [StringLength(20)]
            [Display(Name = "Mã ISBN")]
            public string? Isbn { get; set; }

            [Range(1900, 2100)]
            [Display(Name = "Năm xuất bản")]
            public int? YearOfPublish { get; set; }

            [Display(Name = "Số trang")]
            public int? NumberOfPages { get; set; }

            [StringLength(20)]
            [Display(Name = "Mã vị trí kho (VD: A-01-01)")]
            public string? LocationCode { get; set; }

            [Display(Name = "Mô tả chi tiết sách")]
            public string? Description { get; set; }

            [Display(Name = "Kích hoạt bán ngay")]
            public bool IsActive { get; set; } = true;
        }

        public async Task OnGetAsync()
        {
            Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
                return Page();
            }

            var book = new Book
            {
                Title = Input.Title,
                Author = Input.Author,
                CategoryId = Input.CategoryId,
                Price = Input.Price,
                ImportPrice = Input.ImportPrice,
                StockQuantity = Input.StockQuantity,
                ImageUrl = string.IsNullOrWhiteSpace(Input.ImageUrl) ? "https://via.placeholder.com/200x280?text=MindBook" : Input.ImageUrl,
                Publisher = Input.Publisher,
                Supplier = Input.Supplier,
                Isbn = Input.Isbn,
                YearOfPublish = Input.YearOfPublish,
                NumberOfPages = Input.NumberOfPages,
                LocationCode = Input.LocationCode,
                Description = Input.Description,
                IsActive = Input.IsActive,
                SoldQuantity = 0
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Thêm sách '{book.Title}' thành công!";
            return RedirectToPage("/Admin/Products/Index");
        }
    }
}

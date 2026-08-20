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
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public EditBookInputModel Input { get; set; } = new();

        public Book Book { get; set; } = null!;
        public List<Category> Categories { get; set; } = new();

        public class EditBookInputModel
        {
            public int Id { get; set; }

            [Required(ErrorMessage = "Tiêu đề sách là bắt buộc")]
            [StringLength(300)]
            public string Title { get; set; } = string.Empty;

            public string? Author { get; set; }

            [Required(ErrorMessage = "Vui lòng chọn danh mục")]
            public int CategoryId { get; set; }

            [Required]
            [Range(0, 100000000)]
            public decimal Price { get; set; }

            public decimal ImportPrice { get; set; }

            [Required]
            [Range(0, 100000)]
            public int StockQuantity { get; set; }

            public string? ImageUrl { get; set; }
            public string? Publisher { get; set; }
            public string? Supplier { get; set; }
            public string? Isbn { get; set; }
            public int? YearOfPublish { get; set; }
            public int? NumberOfPages { get; set; }
            public string? LocationCode { get; set; }
            public string? Description { get; set; }
            public bool IsActive { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var book = await _context.Books
                .Include(b => b.DetailImages)
                .Include(b => b.Category)
                .Include(b => b.Location)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null) return NotFound();

            Book = book;
            Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();

            Input = new EditBookInputModel
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                CategoryId = book.CategoryId,
                Price = book.Price,
                ImportPrice = book.ImportPrice,
                StockQuantity = book.StockQuantity,
                ImageUrl = book.ImageUrl,
                Publisher = book.Publisher,
                Supplier = book.Supplier,
                Isbn = book.Isbn,
                YearOfPublish = book.YearOfPublish,
                NumberOfPages = book.NumberOfPages,
                LocationCode = book.LocationCode,
                Description = book.Description,
                IsActive = book.IsActive
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                return await OnGetAsync(id);
            }

            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            book.Title = Input.Title;
            book.Author = Input.Author;
            book.CategoryId = Input.CategoryId;
            book.Price = Input.Price;
            book.ImportPrice = Input.ImportPrice;
            book.StockQuantity = Input.StockQuantity;
            book.ImageUrl = Input.ImageUrl;
            book.Publisher = Input.Publisher;
            book.Supplier = Input.Supplier;
            book.Isbn = Input.Isbn;
            book.YearOfPublish = Input.YearOfPublish;
            book.NumberOfPages = Input.NumberOfPages;
            
            if (!string.IsNullOrWhiteSpace(Input.LocationCode))
            {
                string formattedCode = Input.LocationCode.Trim().ToUpper();
                var loc = await _context.Locations.FirstOrDefaultAsync(l => l.LocationCode == formattedCode);
                book.LocationId = loc?.Id;
            }
            else
            {
                book.LocationId = null;
            }

            book.Description = Input.Description;
            book.IsActive = Input.IsActive;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Cập nhật thông tin sách '{book.Title}' thành công!";
            return RedirectToPage("/Admin/Products/Index");
        }

        public async Task<IActionResult> OnPostAddDetailImageAsync(int id, string detailImageUrl)
        {
            if (!string.IsNullOrWhiteSpace(detailImageUrl))
            {
                _context.BookImages.Add(new BookImage
                {
                    BookId = id,
                    ImageUrl = detailImageUrl.Trim()
                });
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã thêm ảnh chi tiết mới!";
            }

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostDeleteDetailImageAsync(int id, int imageId)
        {
            var img = await _context.BookImages.FirstOrDefaultAsync(bi => bi.Id == imageId && bi.BookId == id);
            if (img != null)
            {
                _context.BookImages.Remove(img);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa ảnh chi tiết!";
            }

            return RedirectToPage(new { id });
        }
    }
}

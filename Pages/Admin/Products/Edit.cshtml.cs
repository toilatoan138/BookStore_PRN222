using BookStore.Data;
using BookStore.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Pages.Admin.Products
{
    [Authorize(Roles = "Admin,Staff")]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Book Input { get; set; } = new();

        // Biến Book dùng để hiển thị dữ liệu gốc lên Sidebar bên phải (Ảnh bìa, Gallery)
        public Book Book { get; set; } = new();

        public List<Category> Categories { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Tải dữ liệu cuốn sách kèm theo Ảnh phụ và thông tin Kho
            var book = await _context.Books
                .Include(b => b.DetailImages)
                .Include(b => b.Location)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy cuốn sách này!";
                return RedirectToPage("./Index");
            }

            // Gán dữ liệu vào biến để hiển thị
            Book = book;
            Input = book;

            Categories = await _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.ParentId).ThenBy(c => c.Name)
                .ToListAsync();

            return Page();
        }

        // 1. HÀM XỬ LÝ LƯU THÔNG TIN CHÍNH (Nút "Lưu thay đổi")
        public async Task<IActionResult> OnPostAsync(int id)
        {
            // Bỏ qua kiểm tra các liên kết object (Tránh lỗi chớp màn hình)
            ModelState.Remove("Input.Category");
            ModelState.Remove("Input.Location");
            ModelState.Remove("Input.SupplierEntity");

            if (!ModelState.IsValid)
            {
                Book = await _context.Books.Include(b => b.DetailImages).FirstOrDefaultAsync(b => b.Id == id) ?? new Book();
                Categories = await _context.Categories.AsNoTracking().ToListAsync();
                return Page();
            }

            var bookToUpdate = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (bookToUpdate == null) return RedirectToPage("./Index");

            // Xử lý biến vị trí kho thủ công
            string? formLocationCode = Request.Form["Input.LocationCode"].ToString();
            if (!string.IsNullOrWhiteSpace(formLocationCode))
            {
                formLocationCode = formLocationCode.Trim();
                var location = await _context.Locations.FirstOrDefaultAsync(l => l.LocationCode == formLocationCode);

                if (location == null)
                {
                    location = new Location { LocationCode = formLocationCode };
                    _context.Locations.Add(location);
                    await _context.SaveChangesAsync();
                }
                bookToUpdate.LocationId = location.Id;
            }
            else
            {
                bookToUpdate.LocationId = null;
            }

            // Cập nhật các trường dữ liệu
            bookToUpdate.Title = Input.Title;
            bookToUpdate.CategoryId = Input.CategoryId;
            bookToUpdate.Author = Input.Author;
            bookToUpdate.Publisher = Input.Publisher;
            bookToUpdate.Price = Input.Price;
            bookToUpdate.ImportPrice = Input.ImportPrice;
            bookToUpdate.StockQuantity = Input.StockQuantity;
            bookToUpdate.ImageUrl = Input.ImageUrl;
            bookToUpdate.Isbn = Input.Isbn;
            bookToUpdate.YearOfPublish = Input.YearOfPublish;
            bookToUpdate.NumberOfPages = Input.NumberOfPages;
            bookToUpdate.Description = Input.Description;
            bookToUpdate.IsActive = Input.IsActive;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã cập nhật thông tin sách thành công!";
            // Cập nhật xong thì load lại trang Edit này để xem thành quả
            return RedirectToPage(new { id = id });
        }

        // 2. HÀM XỬ LÝ THÊM ẢNH PHỤ (Nút "Thêm")
        public async Task<IActionResult> OnPostAddDetailImageAsync(int id, string detailImageUrl)
        {
            if (string.IsNullOrWhiteSpace(detailImageUrl))
                return RedirectToPage(new { id = id });

            var newImage = new BookImage
            {
                BookId = id,
                ImageUrl = detailImageUrl
            };

            _context.Add(newImage);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã thêm ảnh phụ thành công!";
            return RedirectToPage(new { id = id });
        }

        // 3. HÀM XỬ LÝ XÓA ẢNH PHỤ (Nút thùng rác nhỏ)
        public async Task<IActionResult> OnPostDeleteDetailImageAsync(int id, int imageId)
        {
            var image = await _context.FindAsync<BookImage>(imageId);

            if (image != null && image.BookId == id)
            {
                _context.Remove(image);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa ảnh phụ!";
            }

            return RedirectToPage(new { id = id });
        }
    }
}
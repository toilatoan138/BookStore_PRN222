using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        public List<Supplier> Suppliers { get; set; } = new();

        public class BookInputModel
        {
            [Required(ErrorMessage = "Tiêu đề sách là bắt buộc")]
            [StringLength(300, MinimumLength = 1, ErrorMessage = "Tiêu đề sách phải từ 1 đến 300 ký tự")]
            [RegularExpression(@"^[^<>]+$", ErrorMessage = "Tiêu đề không được chứa ký tự HTML (< >)")]
            [Display(Name = "Tiêu đề")]
            public string Title { get; set; } = string.Empty;

            [StringLength(200, ErrorMessage = "Tác giả tối đa 200 ký tự")]
            [RegularExpression(@"^[^<>]+$", ErrorMessage = "Tên tác giả không được chứa ký tự HTML (< >)")]
            [Display(Name = "Tác giả")]
            public string? Author { get; set; }

            [Required(ErrorMessage = "Vui lòng chọn danh mục")]
            [Display(Name = "Danh mục")]
            public int CategoryId { get; set; }

            [Required(ErrorMessage = "Vui lòng chọn nhà cung cấp")]
            [Display(Name = "Nhà cung cấp")]
            public int SupplierId { get; set; }

            [Required(ErrorMessage = "Giá bán là bắt buộc")]
            [Range(1000, 500000000, ErrorMessage = "Giá bán phải từ 1.000 VNĐ đến 500.000.000 VNĐ")]
            [Display(Name = "Giá bán (VNĐ)")]
            public decimal Price { get; set; }

            [Range(0, 500000000, ErrorMessage = "Giá nhập phải từ 0 VNĐ đến 500.000.000 VNĐ")]
            [Display(Name = "Giá nhập (VNĐ)")]
            public decimal ImportPrice { get; set; }

            [StringLength(500, ErrorMessage = "URL Ảnh bìa tối đa 500 ký tự")]
            [Url(ErrorMessage = "Đường dẫn ảnh phải là URL hợp lệ (VD: https://example.com/cover.jpg)")]
            [Display(Name = "URL Ảnh bìa chính")]
            public string? ImageUrl { get; set; }

            [StringLength(200, ErrorMessage = "Nhà xuất bản tối đa 200 ký tự")]
            [RegularExpression(@"^[^<>]+$", ErrorMessage = "Tên nhà xuất bản không được chứa ký tự HTML (< >)")]
            [Display(Name = "Nhà xuất bản")]
            public string? Publisher { get; set; }

            [StringLength(20, ErrorMessage = "Mã ISBN tối đa 20 ký tự")]
            [RegularExpression(@"^[0-9a-zA-Z\-]*$", ErrorMessage = "Mã ISBN chỉ chứa số, chữ cái và dấu gạch nối")]
            [Display(Name = "Mã ISBN")]
            public string? Isbn { get; set; }

            [Range(1800, 2100, ErrorMessage = "Năm xuất bản phải từ năm 1800 đến 2100")]
            [Display(Name = "Năm xuất bản")]
            public int? YearOfPublish { get; set; }

            [Range(1, 50000, ErrorMessage = "Số trang phải từ 1 đến 50.000 trang")]
            [Display(Name = "Số trang")]
            public int? NumberOfPages { get; set; }

            [StringLength(50, ErrorMessage = "Mã vị trí kho tối đa 50 ký tự")]
            [RegularExpression(@"^[a-zA-Z0-9\-]*$", ErrorMessage = "Mã vị trí kho chỉ chứa chữ cái, số và dấu gạch ngang (VD: A-01-01)")]
            [Display(Name = "Mã vị trí kho (VD: A-01-01)")]
            public string? LocationCode { get; set; }

            [StringLength(4000, ErrorMessage = "Mô tả tối đa 4000 ký tự")]
            [Display(Name = "Mô tả chi tiết sách")]
            public string? Description { get; set; }

            [Display(Name = "Kích hoạt bán ngay")]
            public bool IsActive { get; set; } = true;
        }

        public async Task OnGetAsync()
        {
            await LoadDropdownDataAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownDataAsync();
                return Page();
            }

            // 1. Kiểm tra logic kinh doanh: Giá nhập không nên lớn hơn giá bán
            if (Input.ImportPrice > 0 && Input.ImportPrice > Input.Price)
            {
                ModelState.AddModelError("Input.ImportPrice", "Giá nhập không được lớn hơn giá bán.");
                await LoadDropdownDataAsync();
                return Page();
            }

            // 2. Xử lý Vị trí kho (Location)
            int? locationId = null;
            if (!string.IsNullOrWhiteSpace(Input.LocationCode))
            {
                string formLocationCode = Input.LocationCode.Trim().ToUpper();
                var location = await _context.Locations.FirstOrDefaultAsync(l => l.LocationCode == formLocationCode);

                if (location == null)
                {
                    location = new Location { LocationCode = formLocationCode };
                    _context.Locations.Add(location);
                    await _context.SaveChangesAsync();
                }
                locationId = location.Id;
            }

            // 3. Khởi tạo đối tượng Book với dữ liệu đã được làm sạch
            var book = new Book
            {
                Title = Input.Title.Trim(),
                Author = string.IsNullOrWhiteSpace(Input.Author) ? null : Input.Author.Trim(),
                CategoryId = Input.CategoryId,
                SupplierId = Input.SupplierId,
                LocationId = locationId,
                Price = Input.Price,
                ImportPrice = Input.ImportPrice,
                StockQuantity = 0,
                ImageUrl = string.IsNullOrWhiteSpace(Input.ImageUrl) ? "https://via.placeholder.com/200x280?text=MindBook" : Input.ImageUrl.Trim(),
                Publisher = string.IsNullOrWhiteSpace(Input.Publisher) ? null : Input.Publisher.Trim(),
                Isbn = string.IsNullOrWhiteSpace(Input.Isbn) ? null : Input.Isbn.Trim().ToUpper(),
                YearOfPublish = Input.YearOfPublish,
                NumberOfPages = Input.NumberOfPages,
                Description = string.IsNullOrWhiteSpace(Input.Description) ? null : Input.Description.Trim(),
                IsActive = Input.IsActive,
                SoldQuantity = 0
            };

            try
            {
                _context.Books.Add(book);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Đã thêm sách '{book.Title}' thành công!";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi lưu vào hệ thống: " + ex.Message);
                await LoadDropdownDataAsync();
                return Page();
            }
        }

        private async Task LoadDropdownDataAsync()
        {
            Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            Suppliers = await _context.Suppliers.Where(s => s.IsActive == true).OrderBy(s => s.Name).ToListAsync();
        }
    }
}
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
        public Book Input { get; set; } = new();

        // Danh sách Category để đổ vào thẻ <select> trên giao diện
        public List<Category> Categories { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Lấy danh sách danh mục (ưu tiên danh mục con)
            Categories = await _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.ParentId).ThenBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Yêu cầu hệ thống bỏ qua kiểm tra các object liên kết (vì HTML không gửi lên)
            ModelState.Remove("Input.Category");
            ModelState.Remove("Input.Location");
            ModelState.Remove("Input.SupplierEntity");

            // 1. Kiểm tra dữ liệu hợp lệ (không bỏ trống các trường bắt buộc)
            if (!ModelState.IsValid)
            {
                // Nếu lỗi, phải tải lại danh sách Danh mục trước khi trả về form
                Categories = await _context.Categories.AsNoTracking().ToListAsync();
                return Page();
            }

            // 2. Xử lý thủ công biến LocationCode do entity Book chặn gán trực tiếp
            string formLocationCode = Request.Form["Input.LocationCode"].ToString();

            if (!string.IsNullOrWhiteSpace(formLocationCode))
            {
                formLocationCode = formLocationCode.Trim();

                // Tìm xem kho này có trong hệ thống chưa
                var location = await _context.Locations.FirstOrDefaultAsync(l => l.LocationCode == formLocationCode);

                if (location == null)
                {
                    // Tự động tạo vị trí kho mới nếu chưa tồn tại
                    location = new Location { LocationCode = formLocationCode };
                    _context.Locations.Add(location);
                    await _context.SaveChangesAsync();
                }

                Input.LocationId = location.Id;
            }

            // 3. Đưa sách vào Database
            _context.Books.Add(Input);
            await _context.SaveChangesAsync();

            // 4. Báo thành công và quay về trang danh sách
            TempData["SuccessMessage"] = $"Đã thêm sách '{Input.Title}' thành công!";
            return RedirectToPage("./Index");
        }
    }
}
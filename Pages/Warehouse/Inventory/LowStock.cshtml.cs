using BookStore.Data;
using BookStore.Models.Entities;
using BookStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Pages.Warehouse.Inventory
{
    [Authorize(Roles = "Warehouse,Admin")]
    public class LowStockModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWarehouseService _warehouseService;
        private readonly UserManager<ApplicationUser> _userManager;

        public LowStockModel(ApplicationDbContext context, IWarehouseService warehouseService, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _warehouseService = warehouseService;
            _userManager = userManager;
        }

        public List<Book> Books { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public List<string> Publishers { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Keyword { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Publisher { get; set; }

        public async Task OnGetAsync()
        {
            Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            Publishers = await _context.Books
                .Where(b => !string.IsNullOrEmpty(b.Publisher))
                .Select(b => b.Publisher!)
                .Distinct()
                .OrderBy(p => p)
                .ToListAsync();

            var query = _context.Books
                .Include(b => b.Category)
                .Include(b => b.Location)
                .Where(b => b.StockQuantity <= 5 && b.IsActive)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                string kw = Keyword.Trim().ToLower();
                query = query.Where(b => (b.Title != null && b.Title.ToLower().Contains(kw)) ||
                                         (b.Author != null && b.Author.ToLower().Contains(kw)) ||
                                         (b.Location != null && b.Location.LocationCode != null && b.Location.LocationCode.ToLower().Contains(kw)));
            }

            if (CategoryId.HasValue && CategoryId.Value > 0)
            {
                query = query.Where(b => b.CategoryId == CategoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(Publisher))
            {
                query = query.Where(b => b.Publisher == Publisher);
            }

            Books = await query
                .OrderBy(b => b.StockQuantity)
                .ThenBy(b => b.Title)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAdjustStockAsync(int bookId, int newQuantity, string? reason, string? locationCode)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            bool success = await _warehouseService.AdjustStockAsync(bookId, newQuantity, reason, user.Id, locationCode);
            if (success)
            {
                TempData["SuccessMessage"] = "Đã cập nhật số lượng tồn kho thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy sách cần cập nhật.";
            }

            return RedirectToPage(new { keyword = Keyword, categoryId = CategoryId, publisher = Publisher });
        }
    }
}

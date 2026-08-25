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

        public List<Book> LowStockBooks { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public List<string> Publishers { get; set; } = new();
        public List<Branch> Branches { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? FilterBranchId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Keyword { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Publisher { get; set; }

        public async Task OnGetAsync()
        {
            Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            Branches = await _context.Branches.OrderBy(b => b.Name).ToListAsync();
            Publishers = await _context.Books
                .Where(b => !string.IsNullOrEmpty(b.Publisher))
                .Select(b => b.Publisher!)
                .Distinct()
                .OrderBy(p => p)
                .ToListAsync();

            var query = _context.Books
                .Include(b => b.Category)
                .Include(b => b.Location)
                .Include(b => b.BranchInventories)
                    .ThenInclude(bi => bi.Branch)
                .Where(b => b.IsActive)
                .AsSplitQuery()
                .AsNoTracking()
                .AsQueryable();

            if (FilterBranchId.HasValue && FilterBranchId.Value > 0)
            {
                // Low stock in the selected branch means either no record (stock 0) or stock <= 5
                query = query.Where(b => !b.BranchInventories.Any(bi => bi.BranchId == FilterBranchId.Value) || 
                                         b.BranchInventories.Any(bi => bi.BranchId == FilterBranchId.Value && bi.StockQuantity <= 5));
            }
            else
            {
                // Low stock in ANY branch (including no records at all)
                // Wait, if it has no records at all, it's low stock everywhere.
                // It's safer to just say it's low stock if there is any branch <= 5 OR total stock is low?
                // Actually, if a book has no branch inventories at all, it's low stock.
                // Let's just say: Any branch has <=5 OR it has no branch inventories.
                query = query.Where(b => !b.BranchInventories.Any() || b.BranchInventories.Any(bi => bi.StockQuantity <= 5));
            }

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

            LowStockBooks = await query
                .OrderBy(b => b.Title)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAdjustStockAsync(int branchId, int bookId, int newQuantity, string? reason, string? locationCode)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            bool success = await _warehouseService.AdjustStockAsync(branchId, bookId, newQuantity, reason, user.Id, locationCode);
            if (success)
            {
                TempData["SuccessMessage"] = "Đã cập nhật số lượng tồn kho thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy sách cần cập nhật.";
            }

            return RedirectToPage(new { keyword = Keyword, categoryId = CategoryId, publisher = Publisher, filterBranchId = FilterBranchId });
        }
    }
}

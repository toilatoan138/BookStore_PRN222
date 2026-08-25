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
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWarehouseService _warehouseService;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(ApplicationDbContext context, IWarehouseService warehouseService, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _warehouseService = warehouseService;
            _userManager = userManager;
        }

        public List<Book> Books { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public List<string> Authors { get; set; } = new();
        public List<string> Publishers { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Keyword { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Author { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Publisher { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? FilterBranchId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        public int TotalPages { get; set; } = 1;
        public int TotalItems { get; set; } = 0;
        public int PageSize { get; set; } = 10;
        public List<Branch> Branches { get; set; } = new();

        public async Task OnGetAsync()
        {
            Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            Branches = await _context.Branches.OrderBy(b => b.Name).ToListAsync();
            Authors = await _context.Books
                .Where(b => !string.IsNullOrEmpty(b.Author))
                .Select(b => b.Author!)
                .Distinct()
                .OrderBy(a => a)
                .ToListAsync();

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
                .AsSplitQuery()
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

            if (!string.IsNullOrWhiteSpace(Author))
            {
                query = query.Where(b => b.Author == Author);
            }

            if (!string.IsNullOrWhiteSpace(Publisher))
            {
                query = query.Where(b => b.Publisher == Publisher);
            }

            TotalItems = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalItems / (double)PageSize);
            if (TotalPages < 1) TotalPages = 1;
            if (CurrentPage < 1) CurrentPage = 1;
            if (CurrentPage > TotalPages) CurrentPage = TotalPages;

            Books = await query
                .OrderBy(b => b.Title)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAdjustStockAsync(int branchId, int bookId, int newQuantity, string? reason, string? locationCode)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            bool success = await _warehouseService.AdjustStockAsync(branchId, bookId, newQuantity, reason, user.Id, locationCode);
            if (success)
            {
                TempData["SuccessMessage"] = "Đã cập nhật số lượng tồn kho và vị trí kệ thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy sách cần cập nhật.";
            }

            return RedirectToPage(new
            {
                keyword = Keyword,
                categoryId = CategoryId,
                author = Author,
                publisher = Publisher,
                currentPage = CurrentPage
            });
        }
    }
}

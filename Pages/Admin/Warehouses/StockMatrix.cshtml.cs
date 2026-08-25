using BookStore.Data;
using BookStore.Models.Entities;
using BookStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Pages.Admin.Warehouses
{
    [Authorize(Roles = "Admin")]
    public class StockMatrixModel : PageModel
    {
        private readonly IWarehouseAdminService _warehouseAdminService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public StockMatrixModel(
            IWarehouseAdminService warehouseAdminService,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _warehouseAdminService = warehouseAdminService;
            _userManager = userManager;
            _context = context;
        }

        public List<StockMatrixItemDto> Items { get; set; } = new();
        public List<Branch> Branches { get; set; } = new();
        public List<Category> Categories { get; set; } = new();

        public bool IsSuperAdmin { get; set; }
        public int? UserBranchId { get; set; }
        public string UserBranchName { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string? Keyword { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? BranchId { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool LowStockOnly { get; set; } = false;

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        public int TotalItems { get; set; } = 0;
        public int TotalPages { get; set; } = 1;
        public int PageSize { get; set; } = 15;

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return;

            var roleInfo = await _warehouseAdminService.GetUserRoleInfoAsync(user.Id);
            IsSuperAdmin = roleInfo.IsSuperAdmin;
            UserBranchId = roleInfo.BranchId;
            UserBranchName = roleInfo.BranchName;

            Categories = await _context.Categories.OrderBy(c => c.Name).AsNoTracking().ToListAsync();

            var result = await _warehouseAdminService.GetStockMatrixAsync(
                user.Id, Keyword, CategoryId, BranchId, LowStockOnly, CurrentPage, PageSize);

            Items = result.Items;
            TotalItems = result.TotalCount;
            Branches = result.Branches;

            TotalPages = (int)Math.Ceiling(TotalItems / (double)PageSize);
            if (TotalPages < 1) TotalPages = 1;
            if (CurrentPage < 1) CurrentPage = 1;
            if (CurrentPage > TotalPages) CurrentPage = TotalPages;
        }

        public async Task<IActionResult> OnPostAdjustStockAsync(int branchId, int bookId, int newQuantity, string reason)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            var (success, message) = await _warehouseAdminService.AdjustStockAsync(user.Id, branchId, bookId, newQuantity, reason);

            if (success)
            {
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }

            return RedirectToPage(new
            {
                keyword = Keyword,
                categoryId = CategoryId,
                branchId = BranchId,
                lowStockOnly = LowStockOnly,
                currentPage = CurrentPage
            });
        }
    }
}
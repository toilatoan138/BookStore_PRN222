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
    public class TransferModel : PageModel
    {
        private readonly IWarehouseAdminService _warehouseAdminService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public TransferModel(
            IWarehouseAdminService warehouseAdminService,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _warehouseAdminService = warehouseAdminService;
            _userManager = userManager;
            _context = context;
        }

        public bool IsSuperAdmin { get; set; }
        public int? UserBranchId { get; set; }
        public string UserBranchName { get; set; } = string.Empty;

        public List<Branch> Branches { get; set; } = new();
        public List<Book> Books { get; set; } = new();
        public List<BranchInventory> BranchInventories { get; set; } = new();
        public List<InventoryHistory> RecentTransfers { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? SelectedBookId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SelectedFromBranchId { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return;

            var roleInfo = await _warehouseAdminService.GetUserRoleInfoAsync(user.Id);
            IsSuperAdmin = roleInfo.IsSuperAdmin;
            UserBranchId = roleInfo.BranchId;
            UserBranchName = roleInfo.BranchName;

            Branches = await _context.Branches.Where(b => b.IsActive).OrderBy(b => b.Id).AsNoTracking().ToListAsync();
            Books = await _context.Books.Where(b => b.IsActive).OrderBy(b => b.Title).AsNoTracking().ToListAsync();
            BranchInventories = await _context.BranchInventories.AsNoTracking().ToListAsync();

            // Lấy 30 giao dịch điều chuyển gần nhất
            var historyQuery = _context.InventoryHistories
                .Include(h => h.Book)
                .Include(h => h.CreatedBy)
                .Where(h => h.TransactionType == "TRANSFER_OUT" || h.TransactionType == "TRANSFER_IN")
                .AsNoTracking()
                .AsQueryable();

            if (!IsSuperAdmin && UserBranchId.HasValue)
            {
                historyQuery = historyQuery.Where(h => h.RelatedId == UserBranchId.Value || h.CreatedById == user.Id);
            }

            RecentTransfers = await historyQuery
                .OrderByDescending(h => h.CreatedAt)
                .Take(30)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostTransferAsync(
            int fromBranchId, int toBranchId, int bookId, int quantity, string? note)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            var (success, message) = await _warehouseAdminService.TransferStockAsync(
                user.Id, fromBranchId, toBranchId, bookId, quantity, note);

            if (success)
            {
                TempData["SuccessMessage"] = message;
                return RedirectToPage(new { selectedBookId = bookId, selectedFromBranchId = toBranchId });
            }
            else
            {
                TempData["ErrorMessage"] = message;
                return RedirectToPage(new { selectedBookId = bookId, selectedFromBranchId = fromBranchId });
            }
        }
    }
}

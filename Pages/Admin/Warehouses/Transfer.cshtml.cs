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

            var historyQuery = _context.InventoryHistories
                .Include(h => h.Book)
                .Include(h => h.CreatedBy)
                .Where(h => h.TransactionType == "TRANSFER_OUT" || h.TransactionType == "TRANSFER_IN" || h.TransactionType == "IN_TRANSIT")
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

            if (fromBranchId == toBranchId)
            {
                TempData["ErrorMessage"] = "Kho nhận và kho xuất không được trùng nhau!";
                return RedirectToPage(new { selectedBookId = bookId, selectedFromBranchId = fromBranchId });
            }

            if (quantity <= 0)
            {
                TempData["ErrorMessage"] = "Số lượng điều chuyển phải lớn hơn 0!";
                return RedirectToPage(new { selectedBookId = bookId, selectedFromBranchId = fromBranchId });
            }

            var sourceInventory = await _context.BranchInventories
                .FirstOrDefaultAsync(bi => bi.BranchId == fromBranchId && bi.BookId == bookId);

            int availableStock = sourceInventory?.StockQuantity ?? 0;
            if (quantity > availableStock)
            {
                TempData["ErrorMessage"] = $"Số lượng tồn kho không đủ để điều chuyển (Kho xuất hiện chỉ còn {availableStock} cuốn)!";
                return RedirectToPage(new { selectedBookId = bookId, selectedFromBranchId = fromBranchId });
            }

            string? safeNote = string.IsNullOrWhiteSpace(note) ? null : (note.Trim().Length > 500 ? note.Trim().Substring(0, 500) : note.Trim());

            // Gọi Service thực thi nghiệp vụ
            var (success, message) = await _warehouseAdminService.TransferStockAsync(
                user.Id, fromBranchId, toBranchId, bookId, quantity, safeNote);

            if (success)
            {
                // Thay đổi thông báo để chuẩn bị cho logic "In Transit"
                TempData["SuccessMessage"] = "Đã khởi tạo lệnh xuất kho thành công. Hàng đang trong trạng thái luân chuyển (In Transit) chờ kho đích xác nhận!";
                return RedirectToPage(new { selectedBookId = bookId, selectedFromBranchId = fromBranchId });
            }
            else
            {
                TempData["ErrorMessage"] = message;
                return RedirectToPage(new { selectedBookId = bookId, selectedFromBranchId = fromBranchId });
            }
        }
    }
}
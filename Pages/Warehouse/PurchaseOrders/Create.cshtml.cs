using System.ComponentModel.DataAnnotations;
using BookStore.Data;
using BookStore.Models.Entities;
using BookStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Pages.Warehouse.PurchaseOrders
{
    [Authorize(Roles = "Warehouse,Admin")]
    public class CreateModel : PageModel
    {
        private readonly IWarehouseService _warehouseService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateModel(
            IWarehouseService warehouseService,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _warehouseService = warehouseService;
            _context = context;
            _userManager = userManager;
        }

        public List<Supplier> Suppliers { get; set; } = new();
        public List<Book> Books { get; set; } = new();
        public List<Branch> Branches { get; set; } = new();
        public Dictionary<int, Dictionary<int, int>> BookBranchStocks { get; set; } = new();

        [BindProperty]
        public PoCreateInput Input { get; set; } = new();

        public class PoCreateInput
        {
            [Required(ErrorMessage = "Vui lòng chọn chi nhánh nhập hàng")]
            public int BranchId { get; set; }

            [Required(ErrorMessage = "Vui lòng chọn nhà cung cấp")]
            public int SupplierId { get; set; }

            public List<PoItemRow> Items { get; set; } = new();
        }

        public class PoItemRow
        {
            public int BookId { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
        }

        [BindProperty(SupportsGet = true)]
        public int? PrefillBookId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? PrefillBranchId { get; set; }

        public async Task OnGetAsync()
        {
            Suppliers = await _warehouseService.GetSuppliersAsync();
            Books = await _context.Books.OrderBy(b => b.Title).ToListAsync();
            Branches = await _context.Branches.ToListAsync();

            var branchInventories = await _context.BranchInventories.ToListAsync();
            BookBranchStocks = branchInventories
                .GroupBy(bi => bi.BookId)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToDictionary(bi => bi.BranchId, bi => bi.StockQuantity)
                );

            if (PrefillBranchId.HasValue && PrefillBranchId.Value > 0)
            {
                Input.BranchId = PrefillBranchId.Value;
            }

            if (PrefillBookId.HasValue && PrefillBookId.Value > 0)
            {
                var book = Books.FirstOrDefault(b => b.Id == PrefillBookId.Value);
                if (book != null)
                {
                    Input.SupplierId = book.SupplierId ?? 0;
                    Input.Items = new List<PoItemRow>
                    {
                        new PoItemRow
                        {
                            BookId = book.Id,
                            Quantity = 10,
                            UnitPrice = book.Price > 0 ? (book.Price * 0.7m) : 50000 // default price
                        }
                    };
                }
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            if (Input.BranchId <= 0)
            {
                ModelState.AddModelError("Input.BranchId", "Vui lòng chọn chi nhánh nhập hàng.");
            }

            if (Input.SupplierId <= 0)
            {
                ModelState.AddModelError("Input.SupplierId", "Vui lòng chọn nhà cung cấp.");
            }

            var validItems = Input.Items.Where(i => i.BookId > 0 && i.Quantity > 0 && i.UnitPrice > 0).ToList();
            if (!validItems.Any() || !ModelState.IsValid)
            {
                if (!validItems.Any()) ModelState.AddModelError(string.Empty, "Vui lòng thêm ít nhất 1 đầu sách với số lượng và đơn giá hợp lệ.");
                Suppliers = await _warehouseService.GetSuppliersAsync();
                Books = await _context.Books.OrderBy(b => b.Title).ToListAsync();
                Branches = await _context.Branches.ToListAsync();
                return Page();
            }

            var poItems = validItems.Select(i => new PoItemInput
            {
                BookId = i.BookId,
                SupplierId = Input.SupplierId, // Lấy từ form chung
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList();

            var pos = await _warehouseService.CreatePurchaseOrdersAsync(user.Id, poItems, Input.BranchId);
            var poIds = string.Join(", ", pos.Select(p => $"#{p.PurchaseOrderId}"));
            TempData["SuccessMessage"] = $"Đã tạo Đơn nhập hàng {poIds} thành công! Đơn đã được chuyển sang Admin để duyệt.";
            return RedirectToPage("/Warehouse/PurchaseOrders/Index");
        }
    }
}

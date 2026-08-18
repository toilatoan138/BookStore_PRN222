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

        [BindProperty]
        public PoCreateInput Input { get; set; } = new();

        public class PoCreateInput
        {
            [Required(ErrorMessage = "Vui lòng chọn nhà cung cấp")]
            [Display(Name = "Nhà cung cấp")]
            public int SupplierId { get; set; }

            public List<PoItemRow> Items { get; set; } = new();
        }

        public class PoItemRow
        {
            public int BookId { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
        }

        public async Task OnGetAsync()
        {
            Suppliers = await _warehouseService.GetSuppliersAsync();
            Books = await _context.Books.OrderBy(b => b.Title).ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            var validItems = Input.Items.Where(i => i.BookId > 0 && i.Quantity > 0 && i.UnitPrice > 0).ToList();
            if (!validItems.Any())
            {
                ModelState.AddModelError(string.Empty, "Vui lòng thêm ít nhất 1 đầu sách với số lượng và đơn giá hợp lệ.");
                Suppliers = await _warehouseService.GetSuppliersAsync();
                Books = await _context.Books.OrderBy(b => b.Title).ToListAsync();
                return Page();
            }

            var poItems = validItems.Select(i => new PoItemInput
            {
                BookId = i.BookId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList();

            var po = await _warehouseService.CreatePurchaseOrderAsync(Input.SupplierId, user.Id, poItems);
            TempData["SuccessMessage"] = $"Đã tạo Đơn nhập hàng PO #{po.PurchaseOrderId} thành công! Đơn đã được chuyển sang Admin để duyệt.";
            return RedirectToPage("/Warehouse/PurchaseOrders/Index");
        }
    }
}

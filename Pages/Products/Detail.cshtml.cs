using System.ComponentModel.DataAnnotations;
using BookStore.Models.Entities;
using BookStore.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Pages.Products
{
    public class DetailModel : PageModel
    {
        private readonly IBookService _bookService;
        private readonly ICartService _cartService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWarehouseFulfillmentService _fulfillmentService;

        public DetailModel(
            IBookService bookService,
            ICartService cartService,
            UserManager<ApplicationUser> userManager,
            IWarehouseFulfillmentService fulfillmentService)
        {
            _bookService = bookService;
            _cartService = cartService;
            _userManager = userManager;
            _fulfillmentService = fulfillmentService;
        }

        public Book Book { get; set; } = null!;
        public List<Book> SuggestedBooks { get; set; } = new();
        public List<WarehouseStockInfo> WarehouseStocks { get; set; } = new();
        public string CurrentRegion { get; set; } = "Miền Bắc (Hà Nội)";

        [BindProperty]
        public int Quantity { get; set; } = 1;

        [BindProperty]
        public ReviewInputModel ReviewInput { get; set; } = new();

        public class ReviewInputModel
        {
            [Required(ErrorMessage = "Vui lòng chọn số sao đánh giá")]
            [Range(1, 5, ErrorMessage = "Đánh giá từ 1 đến 5 sao")]
            public int Rating { get; set; } = 5;

            [Required(ErrorMessage = "Vui lòng nhập nội dung đánh giá")]
            [StringLength(2000, MinimumLength = 5, ErrorMessage = "Nội dung nhận xét tối thiểu 5 ký tự")]
            public string Comment { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (id <= 0) return NotFound();

            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null || !book.IsActive)
            {
                return NotFound();
            }

            Book = book;
            SuggestedBooks = await _bookService.GetSuggestedBooksAsync(book.Id, book.CategoryId, 6);

            var user = await _userManager.GetUserAsync(User);
            CurrentRegion = _fulfillmentService.GetUserSelectedRegion(HttpContext, user?.Addresses?.FirstOrDefault(a => a.IsDefaultShipping)?.City);
            WarehouseStocks = await _fulfillmentService.GetWarehouseStocksForBookAsync(book.Id, CurrentRegion);

            // Đồng bộ tồn kho chính xác theo thời gian thực từ các kho chi nhánh
            int realTotalStock = WarehouseStocks.Sum(w => w.StockQuantity);
            Book.StockQuantity = realTotalStock;

            return Page();
        }

        public async Task<IActionResult> OnPostAddToCartAsync(int id, int quantity, bool buyNow = false)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToPage("/Account/Login", new { returnUrl = $"/Products/Detail/{id}" });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null || !book.IsActive)
            {
                TempData["ErrorMessage"] = "Sản phẩm không tồn tại hoặc đã ngừng kinh doanh.";
                return RedirectToPage(new { id });
            }

            // Kiểm tra tồn kho thực tế toàn hệ thống
            var userRegion = _fulfillmentService.GetUserSelectedRegion(HttpContext);
            var stocks = await _fulfillmentService.GetWarehouseStocksForBookAsync(id, userRegion);
            int availableStock = stocks.Sum(w => w.StockQuantity);

            if (availableStock <= 0)
            {
                TempData["ErrorMessage"] = "Sản phẩm này hiện đã hết hàng trên toàn bộ các chi nhánh.";
                return RedirectToPage(new { id });
            }

            if (quantity > availableStock)
            {
                TempData["ErrorMessage"] = $"Số lượng yêu cầu ({quantity} cuốn) vượt quá tồn kho thực tế ({availableStock} cuốn). Hệ thống đã tự động thêm tối đa {availableStock} cuốn vào giỏ hàng.";
                quantity = availableStock;
            }

            bool success = await _cartService.AddToCartAsync(user.Id, id, quantity > 0 ? quantity : 1);
            if (!success)
            {
                TempData["ErrorMessage"] = "Không thể thêm vào giỏ hàng (sản phẩm có thể đã hết hàng).";
                return RedirectToPage(new { id });
            }

            if (buyNow)
            {
                return RedirectToPage("/Cart/Index");
            }

            TempData["SuccessMessage"] = $"Đã thêm {quantity} cuốn vào giỏ hàng thành công!";
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostAddReviewAsync(int id)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToPage("/Account/Login", new { returnUrl = $"/Products/Detail/{id}" });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            if (!ModelState.IsValid)
            {
                return await OnGetAsync(id);
            }

            await _bookService.AddReviewAsync(user.Id, id, ReviewInput.Rating, ReviewInput.Comment);
            TempData["SuccessMessage"] = "Cảm ơn bạn đã gửi đánh giá cho sản phẩm này!";

            return RedirectToPage(new { id });
        }
    }
}

using BookStore.Models.Entities;
using BookStore.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IBookService _bookService;
        private readonly ICartService _cartService;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(
            IBookService bookService,
            ICartService cartService,
            UserManager<ApplicationUser> userManager)
        {
            _bookService = bookService;
            _cartService = cartService;
            _userManager = userManager;
        }

        public List<Book> FlashSaleBooks { get; set; } = new();
        public List<Book> BestSellerBooks { get; set; } = new();
        public List<Book> NewArrivalBooks { get; set; } = new();
        public List<Category> Categories { get; set; } = new();

        public async Task OnGetAsync()
        {
            FlashSaleBooks = await _bookService.GetFlashSaleBooksAsync(8);
            BestSellerBooks = await _bookService.GetTopSellingBooksAsync(8);
            NewArrivalBooks = await _bookService.GetNewArrivalsAsync(8);
            Categories = await _bookService.GetCategoriesAsync();

            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    ViewData["CartCount"] = await _cartService.GetCartCountAsync(user.Id);
                }
            }
        }

        public async Task<IActionResult> OnPostAddToCartAsync(int bookId, int quantity = 1, bool buyNow = false)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToPage("/Account/Login", new { returnUrl = "/" });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            bool success = await _cartService.AddToCartAsync(user.Id, bookId, quantity > 0 ? quantity : 1);
            if (success)
            {
                if (buyNow)
                {
                    return RedirectToPage("/Cart/Index");
                }
                TempData["SuccessMessage"] = "Đã thêm sản phẩm vào giỏ hàng!";
            }
            else
            {
                TempData["ErrorMessage"] = "Sản phẩm tạm thời hết hàng hoặc không khả dụng.";
            }

            return RedirectToPage();
        }
    }
}

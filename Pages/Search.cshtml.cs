using BookStore.Models.Entities;
using BookStore.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Pages
{
    public class SearchModel : PageModel
    {
        private readonly IBookService _bookService;
        private readonly ICartService _cartService;
        private readonly UserManager<ApplicationUser> _userManager;

        public SearchModel(
            IBookService bookService,
            ICartService cartService,
            UserManager<ApplicationUser> userManager)
        {
            _bookService = bookService;
            _cartService = cartService;
            _userManager = userManager;
        }

        public PagedResult<Book> PagedBooks { get; set; } = new();
        public List<Category> Categories { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Q { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MinPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MaxPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SortBy { get; set; } = "newest";

        [BindProperty(SupportsGet = true)]
        public int PageIndex { get; set; } = 1;

        public async Task OnGetAsync()
        {
            ViewData["SearchQuery"] = Q;

            var filter = new BookFilterParams
            {
                Keyword = Q,
                CategoryId = CategoryId,
                MinPrice = MinPrice,
                MaxPrice = MaxPrice,
                SortBy = SortBy,
                PageIndex = PageIndex,
                PageSize = 12
            };

            PagedBooks = await _bookService.GetBooksPagedAsync(filter);
            Categories = await _bookService.GetCategoriesAsync();
        }

        public async Task<IActionResult> OnPostAddToCartAsync(int bookId, int quantity = 1)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToPage("/Account/Login", new { returnUrl = Request.Path + Request.QueryString });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            bool success = await _cartService.AddToCartAsync(user.Id, bookId, quantity);
            if (success)
            {
                TempData["SuccessMessage"] = "Đã thêm sản phẩm vào giỏ hàng!";
            }
            else
            {
                TempData["ErrorMessage"] = "Sản phẩm tạm thời hết hàng.";
            }

            return RedirectToPage(new { q = Q, categoryId = CategoryId, minPrice = MinPrice, maxPrice = MaxPrice, sortBy = SortBy, pageIndex = PageIndex });
        }
    }
}

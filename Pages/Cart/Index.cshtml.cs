using BookStore.Models.Entities;
using BookStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Pages.Cart
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ICartService _cartService;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(ICartService cartService, UserManager<ApplicationUser> userManager)
        {
            _cartService = cartService;
            _userManager = userManager;
        }

        public List<CartItem> CartItems { get; set; } = new();
        public decimal GrandTotal => CartItems.Sum(ci => ci.Quantity * ci.Book.Price);

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            CartItems = await _cartService.GetCartItemsAsync(user.Id);
            ViewData["CartCount"] = CartItems.Sum(ci => ci.Quantity);

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateQuantityAsync(int bookId, int quantity)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            await _cartService.UpdateQuantityAsync(user.Id, bookId, quantity);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveAsync(int bookId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            await _cartService.RemoveFromCartAsync(user.Id, bookId);
            TempData["SuccessMessage"] = "Đã xóa sản phẩm khỏi giỏ hàng!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostClearAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            await _cartService.ClearCartAsync(user.Id);
            TempData["SuccessMessage"] = "Đã làm trống giỏ hàng!";
            return RedirectToPage();
        }

        public IActionResult OnPostProceedToCheckout(int[] selectedItems)
        {
            if (selectedItems == null || selectedItems.Length == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ít nhất 1 sản phẩm để tiến hành thanh toán!";
                return RedirectToPage();
            }

            // Save selected item IDs in TempData or query string for Checkout
            TempData["SelectedCheckoutItems"] = string.Join(",", selectedItems);
            return RedirectToPage("/Checkout/Index");
        }
    }
}

using BookStore.Models.Entities;
using BookStore.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Pages.Collections
{
    public class DetailModel : PageModel
    {
        private readonly ICollectionService _collectionService;
        private readonly ICartService _cartService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DetailModel(
            ICollectionService collectionService,
            ICartService cartService,
            UserManager<ApplicationUser> userManager)
        {
            _collectionService = collectionService;
            _cartService = cartService;
            _userManager = userManager;
        }

        public Collection Collection { get; set; } = null!;
        public bool IsOwner { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            string? currentUserId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                currentUserId = user?.Id;
            }

            var col = await _collectionService.GetCollectionByIdAsync(id, currentUserId);
            if (col == null)
            {
                return NotFound();
            }

            Collection = col;
            IsOwner = !string.IsNullOrEmpty(currentUserId) && col.UserId == currentUserId;

            return Page();
        }

        public async Task<IActionResult> OnPostRemoveBookAsync(int id, int bookId)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToPage("/Account/Login");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            await _collectionService.RemoveBookFromCollectionAsync(id, bookId, user.Id);
            TempData["SuccessMessage"] = "Đã xóa sách khỏi bộ sưu tập!";
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostAddToCartAsync(int id, int bookId)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToPage("/Account/Login", new { returnUrl = $"/Collections/Detail/{id}" });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            await _cartService.AddToCartAsync(user.Id, bookId, 1);
            TempData["SuccessMessage"] = "Đã thêm vào giỏ hàng!";
            return RedirectToPage(new { id });
        }
    }
}

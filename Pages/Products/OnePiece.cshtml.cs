using BookStore.Models.Entities;
using BookStore.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Pages.Products
{
    public class OnePieceModel : PageModel
    {
        private readonly IBookService _bookService;

        public OnePieceModel(IBookService bookService)
        {
            _bookService = bookService;
        }

        public List<Book> OnePieceBooks { get; set; } = new();

        public async Task OnGetAsync()
        {
            OnePieceBooks = await _bookService.GetBooksByKeywordAsync("One Piece", 20);
        }
    }
}

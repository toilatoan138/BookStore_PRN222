using BookStore.Models.Entities;
using BookStore.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Pages.Products
{
    public class ConanModel : PageModel
    {
        private readonly IBookService _bookService;

        public ConanModel(IBookService bookService)
        {
            _bookService = bookService;
        }

        public List<Book> ConanBooks { get; set; } = new();

        public async Task OnGetAsync()
        {
            ConanBooks = await _bookService.GetBooksByKeywordAsync("Conan", 20);
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Pages.Staff
{
    [Authorize(Roles = "Staff,Admin")]
    public class ChatModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}

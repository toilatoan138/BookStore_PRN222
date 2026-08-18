using BookStore.Data;
using BookStore.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Pages.Account
{
    public class PublicProfileModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PublicProfileModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public ApplicationUser? TargetUser { get; set; }
        public List<Collection> PublicCollections { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string? username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return NotFound();
            }

            TargetUser = await _userManager.FindByNameAsync(username);
            if (TargetUser == null)
            {
                return NotFound();
            }

            PublicCollections = await _context.Collections
                .Include(c => c.CollectionBooks)
                    .ThenInclude(cb => cb.Book)
                .Where(c => c.UserId == TargetUser.Id && c.IsPublic)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return Page();
        }
    }
}

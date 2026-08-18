using BookStore.Data;
using BookStore.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Pages.Warehouse.Returns
{
    [Authorize(Roles = "Warehouse,Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<ReturnRequest> ReturnRequests { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchName { get; set; }

        [BindProperty(SupportsGet = true)]
        public int StatusFilter { get; set; } = -1;

        public async Task OnGetAsync()
        {
            var query = _context.ReturnRequests
                .Include(r => r.Order)
                    .ThenInclude(o => o.User)
                .Include(r => r.Book)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchName))
            {
                string kw = SearchName.Trim().ToLower();
                query = query.Where(r => (r.Order.FullName != null && r.Order.FullName.ToLower().Contains(kw)) ||
                                         (r.Order.User != null && r.Order.User.FullName != null && r.Order.User.FullName.ToLower().Contains(kw)) ||
                                         (r.Order.PhoneNumber != null && r.Order.PhoneNumber.Contains(kw)));
            }

            if (StatusFilter >= 0)
            {
                query = query.Where(r => r.Status == StatusFilter);
            }

            ReturnRequests = await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
    }
}

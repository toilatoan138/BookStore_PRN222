using BookStore.Data;
using BookStore.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Pages.Warehouse.Inventory
{
    [Authorize(Roles = "Warehouse,Admin")]
    public class HistoryModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public HistoryModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<InventoryHistory> Histories { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Filter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Keyword { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.InventoryHistories
                .Include(h => h.Book)
                .Include(h => h.CreatedBy)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(Filter))
            {
                string f = Filter.Trim().ToUpper();
                query = query.Where(h => h.TransactionType.ToUpper() == f);
            }

            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                string kw = Keyword.Trim().ToLower();
                query = query.Where(h => (h.Book != null && h.Book.Title != null && h.Book.Title.ToLower().Contains(kw)) ||
                                         (h.Book != null && h.Book.LocationCode != null && h.Book.LocationCode.ToLower().Contains(kw)));
            }

            Histories = await query
                .OrderByDescending(h => h.CreatedAt)
                .Take(100)
                .ToListAsync();
        }
    }
}

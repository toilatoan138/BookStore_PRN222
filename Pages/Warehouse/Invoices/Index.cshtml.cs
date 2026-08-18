using BookStore.Data;
using BookStore.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Pages.Warehouse.Invoices
{
    [Authorize(Roles = "Warehouse,Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Invoice> Invoices { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Filter { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.Invoices
                .Include(i => i.Order)
                    .ThenInclude(o => o!.Details)
                    .ThenInclude(od => od.Book)
                .Include(i => i.Order)
                    .ThenInclude(o => o!.User)
                .Include(i => i.PurchaseOrder)
                    .ThenInclude(po => po!.Details)
                    .ThenInclude(pod => pod.Book)
                .Include(i => i.PurchaseOrder)
                    .ThenInclude(po => po!.Supplier)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(Filter))
            {
                string f = Filter.Trim().ToUpper();
                if (f == "SALE" || f == "PURCHASE")
                {
                    query = query.Where(i => i.InvoiceType.ToUpper() == f);
                }
            }

            Invoices = await query
                .OrderByDescending(i => i.CreatedDate)
                .ToListAsync();
        }
    }
}

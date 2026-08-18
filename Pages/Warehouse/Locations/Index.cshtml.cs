using System.ComponentModel.DataAnnotations;
using BookStore.Data;
using BookStore.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Pages.Warehouse.Locations
{
    [Authorize(Roles = "Warehouse,Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Location> Locations { get; set; } = new();
        public List<string> ZoneList { get; set; } = new();
        public List<Category> Categories { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SelectedZone { get; set; }

        [BindProperty]
        public LocationInputModel Input { get; set; } = new();

        public class LocationInputModel
        {
            public int? Id { get; set; }

            [Required(ErrorMessage = "Khu vực là bắt buộc")]
            [StringLength(10)]
            [Display(Name = "Khu vực (Zone)")]
            public string Zone { get; set; } = "A";

            [Required(ErrorMessage = "Mã dãy kệ là bắt buộc")]
            [StringLength(10)]
            [Display(Name = "Kệ (Rack)")]
            public string Rack { get; set; } = "01";

            [Required(ErrorMessage = "Tầng kệ là bắt buộc")]
            [StringLength(10)]
            [Display(Name = "Tầng (Shelf)")]
            public string Shelf { get; set; } = "01";

            public int? CategoryId { get; set; }

            [StringLength(500)]
            [Display(Name = "Ghi chú")]
            public string? Description { get; set; }
        }

        public async Task OnGetAsync()
        {
            Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            ZoneList = await _context.Locations
                .Where(l => !string.IsNullOrEmpty(l.Zone))
                .Select(l => l.Zone!)
                .Distinct()
                .OrderBy(z => z)
                .ToListAsync();

            var query = _context.Locations
                .Include(l => l.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(SelectedZone))
            {
                query = query.Where(l => l.Zone == SelectedZone);
            }

            Locations = await query.OrderBy(l => l.LocationCode).ToListAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            string zone = Input.Zone.Trim().ToUpper();
            string rack = Input.Rack.Trim().ToUpper().PadLeft(2, '0');
            string shelf = Input.Shelf.Trim().ToUpper().PadLeft(2, '0');
            string code = $"{zone}-{rack}-{shelf}";

            bool exists = await _context.Locations.AnyAsync(l => l.LocationCode == code);
            if (exists)
            {
                TempData["ErrorMessage"] = $"Mã vị trí '{code}' đã tồn tại trong sơ đồ kho!";
                return RedirectToPage(new { selectedZone = SelectedZone });
            }

            var loc = new Location
            {
                Zone = zone,
                Rack = rack,
                Shelf = shelf,
                LocationCode = code,
                CategoryId = Input.CategoryId > 0 ? Input.CategoryId : null,
                Description = Input.Description
            };

            _context.Locations.Add(loc);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã tạo vị trí kho mới '{code}'!";
            return RedirectToPage(new { selectedZone = SelectedZone });
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            if (!Input.Id.HasValue) return RedirectToPage(new { selectedZone = SelectedZone });

            var loc = await _context.Locations.FindAsync(Input.Id.Value);
            if (loc != null)
            {
                string zone = Input.Zone.Trim().ToUpper();
                string rack = Input.Rack.Trim().ToUpper().PadLeft(2, '0');
                string shelf = Input.Shelf.Trim().ToUpper().PadLeft(2, '0');
                string code = $"{zone}-{rack}-{shelf}";

                loc.Zone = zone;
                loc.Rack = rack;
                loc.Shelf = shelf;
                loc.LocationCode = code;
                loc.CategoryId = Input.CategoryId > 0 ? Input.CategoryId : null;
                loc.Description = Input.Description;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã cập nhật thông tin vị trí '{code}' thành công!";
            }

            return RedirectToPage(new { selectedZone = SelectedZone });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var loc = await _context.Locations.FindAsync(id);
            if (loc != null)
            {
                bool hasBooks = await _context.Books.AnyAsync(b => b.LocationId == id);
                if (hasBooks)
                {
                    TempData["ErrorMessage"] = "Không thể xóa vị trí này vì đang có sách lưu trữ tại đây!";
                    return RedirectToPage(new { selectedZone = SelectedZone });
                }

                _context.Locations.Remove(loc);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa vị trí kho thành công!";
            }
            return RedirectToPage(new { selectedZone = SelectedZone });
        }
    }
}

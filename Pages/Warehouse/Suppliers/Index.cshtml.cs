using System.ComponentModel.DataAnnotations;
using BookStore.Data;
using BookStore.Models.Entities;
using BookStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Pages.Warehouse.Suppliers
{
    [Authorize(Roles = "Warehouse,Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWarehouseService _warehouseService;

        public IndexModel(ApplicationDbContext context, IWarehouseService warehouseService)
        {
            _context = context;
            _warehouseService = warehouseService;
        }

        public List<Supplier> Suppliers { get; set; } = new();
        public List<Supplier> DeletedSuppliers { get; set; } = new();

        [BindProperty]
        public SupplierInputModel Input { get; set; } = new();

        public class SupplierInputModel
        {
            public int? Id { get; set; }

            [Required(ErrorMessage = "Tên nhà cung cấp là bắt buộc")]
            [StringLength(200)]
            [Display(Name = "Tên nhà cung cấp")]
            public string Name { get; set; } = string.Empty;

            [StringLength(100)]
            [Display(Name = "Người đại diện")]
            public string? ContactPerson { get; set; }

            [StringLength(15)]
            [Display(Name = "Số điện thoại")]
            public string? Phone { get; set; }

            [StringLength(100)]
            [Display(Name = "Email liên hệ")]
            public string? Email { get; set; }

            [StringLength(500)]
            [Display(Name = "Địa chỉ trụ sở / Kho")]
            public string? Address { get; set; }
        }

        public async Task OnGetAsync()
        {
            Suppliers = await _context.Suppliers
                .Where(s => s.IsActive == true)
                .OrderBy(s => s.Name)
                .ToListAsync();

            DeletedSuppliers = await _context.Suppliers
                .Where(s => s.IsActive == false)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            var supplier = new Supplier
            {
                Name = Input.Name,
                ContactPerson = Input.ContactPerson,
                Phone = Input.Phone,
                Email = Input.Email,
                Address = Input.Address,
                IsActive = true
            };

            await _warehouseService.CreateSupplierAsync(supplier);
            TempData["SuccessMessage"] = $"Thêm nhà cung cấp '{supplier.Name}' thành công!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            if (!Input.Id.HasValue) return RedirectToPage();

            var supplier = await _context.Suppliers.FindAsync(Input.Id.Value);
            if (supplier != null)
            {
                supplier.Name = Input.Name;
                supplier.ContactPerson = Input.ContactPerson;
                supplier.Phone = Input.Phone;
                supplier.Email = Input.Email;
                supplier.Address = Input.Address;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật thông tin nhà cung cấp thành công!";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier != null)
            {
                supplier.IsActive = false;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã chuyển nhà cung cấp '{supplier.Name}' vào kho lưu trữ!";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRestoreAsync(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier != null)
            {
                supplier.IsActive = true;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã khôi phục nhà cung cấp '{supplier.Name}' thành công!";
            }
            return RedirectToPage();
        }
    }
}

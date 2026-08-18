using BookStore.Models.Entities;
using BookStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Pages.Warehouse.PurchaseOrders
{
    [Authorize(Roles = "Warehouse,Admin")]
    public class ReceiveModel : PageModel
    {
        private readonly IWarehouseService _warehouseService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReceiveModel(IWarehouseService warehouseService, UserManager<ApplicationUser> userManager)
        {
            _warehouseService = warehouseService;
            _userManager = userManager;
        }

        public PurchaseOrder PurchaseOrder { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var po = await _warehouseService.GetPurchaseOrderByIdAsync(id);
            if (po == null) return NotFound();

            PurchaseOrder = po;
            return Page();
        }

        public async Task<IActionResult> OnPostConfirmAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            bool success = await _warehouseService.ReceivePurchaseOrderGoodsAsync(id, user.Id);
            if (success)
            {
                TempData["SuccessMessage"] = $"Đã nhập kho thành công cho Đơn PO #{id}! Số lượng tồn kho và phiếu nhập đã được cập nhật.";
                return RedirectToPage("/Warehouse/PurchaseOrders/Index");
            }

            TempData["ErrorMessage"] = "Không thể nhận hàng cho đơn PO này (Đơn phải ở trạng thái Đã duyệt bởi Admin).";
            return RedirectToPage(new { id });
        }
    }
}

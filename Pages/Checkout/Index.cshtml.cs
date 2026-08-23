using System.ComponentModel.DataAnnotations;
using BookStore.Data;
using BookStore.Models.Entities;
using BookStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Pages.Checkout
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;
        private readonly ICartService _cartService;
        private readonly IVnPayService _vnPayService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public IndexModel(
            IOrderService orderService,
            IUserService userService,
            ICartService cartService,
            IVnPayService vnPayService,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _orderService = orderService;
            _userService = userService;
            _cartService = cartService;
            _vnPayService = vnPayService;
            _userManager = userManager;
            _context = context;
        }

        public ApplicationUser CurrentUser { get; set; } = null!;
        public List<Address> Addresses { get; set; } = new();
        public List<CartItem> CheckoutItems { get; set; } = new();
        public List<UserVoucher> AvailableVouchers { get; set; } = new();
        public CheckoutCalculationResult Calculation { get; set; } = new();
        public bool RequiresOrderSplit { get; set; } = false;

        [BindProperty]
        public CheckoutFormModel Form { get; set; } = new();

        public class CheckoutFormModel
        {
            [Required(ErrorMessage = "Vui lòng chọn địa chỉ giao hàng")]
            public int SelectedAddressId { get; set; }

            [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
            public string PaymentMethod { get; set; } = "COD"; // COD, VNPAY, WALLET

            public string? VoucherCode { get; set; }

            public string SelectedBookIdsCsv { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync(string? items, string? voucher)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            CurrentUser = user;
            Addresses = await _userService.GetAddressesByUserIdAsync(user.Id);

            // Get selected book IDs from param or TempData
            string? idsCsv = items ?? TempData["SelectedCheckoutItems"]?.ToString();
            var allCart = await _cartService.GetCartItemsAsync(user.Id);

            if (!string.IsNullOrEmpty(idsCsv))
            {
                var idList = idsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                   .Select(s => int.TryParse(s, out var i) ? i : 0)
                                   .Where(i => i > 0)
                                   .ToList();

                CheckoutItems = allCart.Where(ci => idList.Contains(ci.BookId)).ToList();
            }
            else
            {
                CheckoutItems = allCart;
            }

            if (!CheckoutItems.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng thanh toán trống. Vui lòng chọn sản phẩm.";
                return RedirectToPage("/Cart/Index");
            }

            Form.SelectedBookIdsCsv = string.Join(",", CheckoutItems.Select(ci => ci.BookId));
            Form.VoucherCode = voucher;

            if (Addresses.Any(a => a.IsDefaultShipping))
            {
                Form.SelectedAddressId = Addresses.First(a => a.IsDefaultShipping).Id;
            }
            else if (Addresses.Any())
            {
                Form.SelectedAddressId = Addresses.First().Id;
            }

            // --- CHECK FOR ORDER SPLIT ---
            var bookIds = CheckoutItems.Select(ci => ci.BookId).ToList();
            var allInventories = await _context.BranchInventories
                .Where(bi => bookIds.Contains(bi.BookId))
                .ToListAsync();

            bool canFulfillInOneBranch = false;
            var branchGroups = allInventories.GroupBy(bi => bi.BranchId);
            foreach(var group in branchGroups)
            {
                bool thisBranchCanFulfillAll = true;
                foreach(var item in CheckoutItems)
                {
                    var branchStock = group.FirstOrDefault(bi => bi.BookId == item.BookId)?.StockQuantity ?? 0;
                    if (branchStock < item.Quantity)
                    {
                        thisBranchCanFulfillAll = false;
                        break;
                    }
                }
                if (thisBranchCanFulfillAll)
                {
                    canFulfillInOneBranch = true;
                    break;
                }
            }
            RequiresOrderSplit = !canFulfillInOneBranch;
            // -----------------------------

            // Load user vouchers
            AvailableVouchers = await _context.UserVouchers
                .Include(uv => uv.Voucher)
                .Where(uv => uv.UserId == user.Id && !uv.IsUsed && uv.Voucher.Status == 1 && uv.Voucher.EndDate >= DateTime.UtcNow)
                .ToListAsync();

            Calculation = await _orderService.CalculateCheckoutAsync(
                user.Id,
                CheckoutItems.Select(ci => ci.BookId).ToList(),
                Form.VoucherCode,
                Form.PaymentMethod
            );

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            var bookIds = Form.SelectedBookIdsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                .Select(s => int.TryParse(s, out var i) ? i : 0)
                                                .Where(i => i > 0)
                                                .ToList();

            if (!bookIds.Any())
            {
                TempData["ErrorMessage"] = "Không có sản phẩm nào được chọn.";
                return RedirectToPage("/Cart/Index");
            }

            if (Form.SelectedAddressId <= 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn địa chỉ giao hàng!";
                return await OnGetAsync(Form.SelectedBookIdsCsv, Form.VoucherCode);
            }

            // If payment is VNPay, redirect to VNPay payment URL
            if (Form.PaymentMethod.Equals("VNPAY", StringComparison.OrdinalIgnoreCase))
            {
                var calc = await _orderService.CalculateCheckoutAsync(user.Id, bookIds, Form.VoucherCode, "VNPAY");

                string vnpayOrderId = $"ORDER_{DateTime.UtcNow:yyyyMMddHHmmss}_{new Random().Next(100, 999)}";

                // Save pending checkout state in Session for callback
                HttpContext.Session.SetString("VnPay_UserId", user.Id);
                HttpContext.Session.SetString("VnPay_BookIds", Form.SelectedBookIdsCsv);
                HttpContext.Session.SetInt32("VnPay_AddressId", Form.SelectedAddressId);
                HttpContext.Session.SetString("VnPay_VoucherCode", Form.VoucherCode ?? "");
                HttpContext.Session.SetString("VnPay_TxnRef", vnpayOrderId);

                string returnUrl = Url.Page("/Checkout/VnPayReturn", pageHandler: null, values: null, protocol: Request.Scheme)!;

                var paymentReq = new VnPayPaymentRequest
                {
                    OrderId = vnpayOrderId,
                    Amount = calc.FinalTotal,
                    OrderInfo = $"Thanh toan don hang {vnpayOrderId} tai MindBook",
                    ReturnUrl = returnUrl,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1"
                };

                string paymentUrl = _vnPayService.CreatePaymentUrl(paymentReq);
                return Redirect(paymentUrl);
            }

            // Normal Checkout (COD or WALLET)
            var createReq = new CreateOrderRequest
            {
                UserId = user.Id,
                SelectedBookIds = bookIds,
                AddressId = Form.SelectedAddressId,
                PaymentMethod = Form.PaymentMethod,
                VoucherCode = Form.VoucherCode
            };

            var (success, message, orderId) = await _orderService.CreateOrderAsync(createReq);

            if (!success)
            {
                TempData["ErrorMessage"] = message;
                return await OnGetAsync(Form.SelectedBookIdsCsv, Form.VoucherCode);
            }

            TempData["SuccessMessage"] = "Đặt hàng thành công! Cảm ơn bạn đã mua sách tại MindBook.";
            return RedirectToPage("/Orders/Detail", new { id = orderId });
        }
    }
}

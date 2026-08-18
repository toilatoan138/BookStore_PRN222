using System.ComponentModel.DataAnnotations;
using BookStore.Models.Entities;
using BookStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Pages.Account.Addresses
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserService _userService;

        public IndexModel(UserManager<ApplicationUser> userManager, IUserService userService)
        {
            _userManager = userManager;
            _userService = userService;
        }

        public List<Address> Addresses { get; set; } = new();

        [BindProperty]
        public AddressInputModel Input { get; set; } = new();

        public class AddressInputModel
        {
            public int? Id { get; set; }

            [Required(ErrorMessage = "Họ tên người nhận là bắt buộc")]
            [StringLength(100)]
            [Display(Name = "Họ tên người nhận")]
            public string FullName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
            [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
            [Display(Name = "Số điện thoại")]
            public string Phone { get; set; } = string.Empty;

            [Required(ErrorMessage = "Tỉnh/Thành phố là bắt buộc")]
            [StringLength(100)]
            [Display(Name = "Tỉnh / Thành phố")]
            public string City { get; set; } = string.Empty;

            [Required(ErrorMessage = "Quận/Huyện là bắt buộc")]
            [StringLength(100)]
            [Display(Name = "Quận / Huyện")]
            public string District { get; set; } = string.Empty;

            [Required(ErrorMessage = "Phường/Xã là bắt buộc")]
            [StringLength(100)]
            [Display(Name = "Phường / Xã")]
            public string Ward { get; set; } = string.Empty;

            [Required(ErrorMessage = "Địa chỉ cụ thể là bắt buộc")]
            [StringLength(300)]
            [Display(Name = "Địa chỉ chi tiết (Số nhà, tên đường...)")]
            public string AddressDetail { get; set; } = string.Empty;

            [Display(Name = "Đặt làm địa chỉ giao hàng mặc định")]
            public bool IsDefaultShipping { get; set; } = false;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            Addresses = await _userService.GetAddressesByUserIdAsync(user.Id);
            return Page();
        }

        public async Task<IActionResult> OnPostAddAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            if (!ModelState.IsValid)
            {
                Addresses = await _userService.GetAddressesByUserIdAsync(user.Id);
                return Page();
            }

            var address = new Address
            {
                UserId = user.Id,
                FullName = Input.FullName,
                Phone = Input.Phone,
                City = Input.City,
                District = Input.District,
                Ward = Input.Ward,
                AddressDetail = Input.AddressDetail,
                IsDefaultShipping = Input.IsDefaultShipping,
                IsDefaultBilling = Input.IsDefaultShipping
            };

            await _userService.AddAddressAsync(address);
            TempData["SuccessMessage"] = "Thêm địa chỉ giao hàng mới thành công!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            if (!Input.Id.HasValue || Input.Id.Value <= 0)
            {
                TempData["ErrorMessage"] = "Không tìm thấy địa chỉ cần sửa.";
                return RedirectToPage();
            }

            var address = new Address
            {
                Id = Input.Id.Value,
                UserId = user.Id,
                FullName = Input.FullName,
                Phone = Input.Phone,
                City = Input.City,
                District = Input.District,
                Ward = Input.Ward,
                AddressDetail = Input.AddressDetail,
                IsDefaultShipping = Input.IsDefaultShipping
            };

            await _userService.UpdateAddressAsync(address);
            TempData["SuccessMessage"] = "Cập nhật địa chỉ thành công!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int addressId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            await _userService.DeleteAddressAsync(addressId, user.Id);
            TempData["SuccessMessage"] = "Xóa địa chỉ thành công!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSetDefaultAsync(int addressId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            await _userService.SetDefaultShippingAddressAsync(addressId, user.Id);
            TempData["SuccessMessage"] = "Đã đổi địa chỉ giao hàng mặc định!";
            return RedirectToPage();
        }
    }
}

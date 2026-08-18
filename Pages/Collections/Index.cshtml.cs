using System.ComponentModel.DataAnnotations;
using BookStore.Models.Entities;
using BookStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Pages.Collections
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ICollectionService _collectionService;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(ICollectionService collectionService, UserManager<ApplicationUser> userManager)
        {
            _collectionService = collectionService;
            _userManager = userManager;
        }

        public List<Collection> Collections { get; set; } = new();

        [BindProperty]
        public CreateCollectionInput Input { get; set; } = new();

        public class CreateCollectionInput
        {
            [Required(ErrorMessage = "Tên bộ sưu tập là bắt buộc")]
            [StringLength(200)]
            [Display(Name = "Tên bộ sưu tập")]
            public string Name { get; set; } = string.Empty;

            [StringLength(500)]
            [Display(Name = "Mô tả")]
            public string? Description { get; set; }

            [Display(Name = "Công khai bộ sưu tập")]
            public bool IsPublic { get; set; } = false;

            [Display(Name = "Màu bìa chủ đạo")]
            public string CoverColor { get; set; } = "#C92127";
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            Collections = await _collectionService.GetUserCollectionsAsync(user.Id);
            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            if (!ModelState.IsValid)
            {
                Collections = await _collectionService.GetUserCollectionsAsync(user.Id);
                return Page();
            }

            await _collectionService.CreateCollectionAsync(
                user.Id,
                Input.Name,
                Input.Description,
                Input.IsPublic,
                Input.CoverColor
            );

            TempData["SuccessMessage"] = "Tạo bộ sưu tập mới thành công!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            await _collectionService.DeleteCollectionAsync(id, user.Id);
            TempData["SuccessMessage"] = "Đã xóa bộ sưu tập!";
            return RedirectToPage();
        }
    }
}

using System.ComponentModel.DataAnnotations;
using BookStore.Models.Entities;
using BookStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Pages.Staff.Customers
{
    [Authorize(Roles = "Staff,Admin")]
    public class DetailModel : PageModel
    {
        private readonly IStaffService _staffService;
        private readonly IUserService _userService;

        public DetailModel(IStaffService staffService, IUserService userService)
        {
            _staffService = staffService;
            _userService = userService;
        }

        public ApplicationUser Customer { get; set; } = null!;
        public List<CustomerNote> Notes { get; set; } = new();

        [BindProperty]
        public NoteInputModel NoteInput { get; set; } = new();

        [BindProperty]
        public FPointAdjustModel FPointInput { get; set; } = new();

        public class NoteInputModel
        {
            public string ContactChannel { get; set; } = "Điện thoại";

            [Required(ErrorMessage = "Nội dung ghi chú là bắt buộc")]
            [StringLength(2000)]
            public string NoteContent { get; set; } = string.Empty;

            [DataType(DataType.Date)]
            public DateTime? FollowUpDate { get; set; }
        }

        public class FPointAdjustModel
        {
            [Required]
            public string ActionType { get; set; } = "add"; // add / sub

            [Required]
            [Range(1, 1000000)]
            public int Amount { get; set; } = 50;

            [Required(ErrorMessage = "Lý do cộng/trừ điểm là bắt buộc")]
            public string Reason { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var customer = await _staffService.GetCustomerDetailAsync(id);
            if (customer == null) return NotFound();

            Customer = customer;
            Notes = await _staffService.GetCustomerNotesAsync(id);

            return Page();
        }

        public async Task<IActionResult> OnPostAddNoteAsync(string id)
        {
            if (!ModelState.IsValid)
            {
                return await OnGetAsync(id);
            }

            await _staffService.AddCustomerNoteAsync(id, NoteInput.ContactChannel, NoteInput.NoteContent, NoteInput.FollowUpDate);
            TempData["SuccessMessage"] = "Đã lưu ghi chú chăm sóc khách hàng!";
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostAdjustFPointsAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(FPointInput.Reason))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập lý do điều chỉnh điểm.";
                return RedirectToPage(new { id });
            }

            await _userService.AddFPointsAsync(id, FPointInput.Amount, FPointInput.Reason, FPointInput.ActionType);
            TempData["SuccessMessage"] = $"Đã {(FPointInput.ActionType == "add" ? "cộng" : "trừ")} {FPointInput.Amount} F-Points thành công!";
            return RedirectToPage(new { id });
        }
    }
}

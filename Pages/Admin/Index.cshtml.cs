using BookStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IAdminService _adminService;

        public IndexModel(IAdminService adminService)
        {
            _adminService = adminService;
        }

        public AdminDashboardStats Stats { get; set; } = new();

        public async Task OnGetAsync()
        {
            Stats = await _adminService.GetDashboardStatsAsync();
        }
    }
}

using System.Text;
using BookStore.Data;
using BookStore.Models.Entities;
using BookStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Pages.Admin.Warehouses
{
    [Authorize(Roles = "Admin")]
    public class ReportsModel : PageModel
    {
        private readonly IWarehouseAdminService _warehouseAdminService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ReportsModel(
            IWarehouseAdminService warehouseAdminService,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _warehouseAdminService = warehouseAdminService;
            _userManager = userManager;
            _context = context;
        }

        public bool IsSuperAdmin { get; set; }
        public int? UserBranchId { get; set; }
        public string UserBranchName { get; set; } = string.Empty;

        public List<Branch> Branches { get; set; } = new();
        public List<XntReportItemDto> ReportItems { get; set; } = new();
        public List<InventoryHistory> HistoryList { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? BranchId { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FromDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? ToDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? TransactionType { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Keyword { get; set; }

        // Tổng hợp
        public int TotalImportAll { get; set; } = 0;
        public int TotalExportAll { get; set; } = 0;
        public int TotalTransferInAll { get; set; } = 0;
        public int TotalTransferOutAll { get; set; } = 0;
        public int TotalAdjustmentAll { get; set; } = 0;
        public int TotalCurrentStockAll { get; set; } = 0;

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return;

            var roleInfo = await _warehouseAdminService.GetUserRoleInfoAsync(user.Id);
            IsSuperAdmin = roleInfo.IsSuperAdmin;
            UserBranchId = roleInfo.BranchId;
            UserBranchName = roleInfo.BranchName;

            Branches = await _context.Branches.Where(b => b.IsActive).OrderBy(b => b.Id).AsNoTracking().ToListAsync();

            if (!IsSuperAdmin && UserBranchId.HasValue)
            {
                BranchId = UserBranchId.Value;
            }

            ReportItems = await _warehouseAdminService.GetXntReportAsync(user.Id, BranchId, FromDate, ToDate);

            TotalImportAll = ReportItems.Sum(r => r.TotalImport);
            TotalExportAll = ReportItems.Sum(r => r.TotalExport);
            TotalTransferInAll = ReportItems.Sum(r => r.TotalTransferIn);
            TotalTransferOutAll = ReportItems.Sum(r => r.TotalTransferOut);
            TotalAdjustmentAll = ReportItems.Sum(r => r.TotalAdjustment);
            TotalCurrentStockAll = ReportItems.Sum(r => r.CurrentStock);

            HistoryList = await _warehouseAdminService.GetHistoryAsync(
                user.Id, BranchId, TransactionType, FromDate, ToDate, Keyword, 100);
        }

        public async Task<IActionResult> OnGetExportCsvAsync(int? branchId, DateTime? fromDate, DateTime? toDate)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            var items = await _warehouseAdminService.GetXntReportAsync(user.Id, branchId, fromDate, toDate);

            var sb = new StringBuilder();
            sb.AppendLine("ID Sách,Tên Sách,Danh Mục,Nhập Kho (PO),Xuất Bán (Orders),Nhận Chuyển Kho,Xuất Chuyển Kho,Điều Chỉnh Kiểm Kê,Tồn Kho Hiện Tại");

            foreach (var item in items)
            {
                string titleEscaped = $"\"{item.Title.Replace("\"", "\"\"")}\"";
                string catEscaped = $"\"{item.CategoryName?.Replace("\"", "\"\"") ?? "N/A"}\"";
                sb.AppendLine($"{item.BookId},{titleEscaped},{catEscaped},{item.TotalImport},{item.TotalExport},{item.TotalTransferIn},{item.TotalTransferOut},{item.TotalAdjustment},{item.CurrentStock}");
            }

            // UTF-8 BOM for Excel Vietnamese compatibility
            var preamble = Encoding.UTF8.GetPreamble();
            var data = Encoding.UTF8.GetBytes(sb.ToString());
            var result = new byte[preamble.Length + data.Length];
            Buffer.BlockCopy(preamble, 0, result, 0, preamble.Length);
            Buffer.BlockCopy(data, 0, result, preamble.Length, data.Length);

            string fileName = $"BaoCao_XNT_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            return File(result, "text/csv", fileName);
        }
    }
}

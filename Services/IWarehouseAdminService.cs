using BookStore.Models.Entities;

namespace BookStore.Services
{
    public class AdminWarehouseOverviewDto
    {
        public bool IsSuperAdmin { get; set; }
        public int? UserBranchId { get; set; }
        public string? UserBranchName { get; set; }
        public int TotalSkus { get; set; }
        public int TotalBooksInStock { get; set; }
        public decimal TotalCostValue { get; set; }
        public decimal TotalRetailValue { get; set; }
        public int LowStockCount { get; set; }
        public int TotalBranches { get; set; }
        public int InconsistentBookCount { get; set; }
        public List<BranchStockSummaryDto> BranchSummaries { get; set; } = new();
        public List<Book> LowStockBooks { get; set; } = new();
        public List<InventoryHistory> RecentMovements { get; set; } = new();
    }

    public class BranchStockSummaryDto
    {
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
        public int TotalStock { get; set; }
        public int TotalSkus { get; set; }
        public decimal CostValue { get; set; }
        public decimal RetailValue { get; set; }
        public string? ManagerName { get; set; }
    }

    public class StockMatrixItemDto
    {
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Author { get; set; }
        public string? ImageUrl { get; set; }
        public string? CategoryName { get; set; }
        public decimal Price { get; set; }
        public decimal ImportPrice { get; set; }
        public int TotalStock { get; set; }
        public Dictionary<int, int> BranchStocks { get; set; } = new();
    }

    public class XntReportItemDto
    {
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? CategoryName { get; set; }
        public int OpeningStock { get; set; }
        public int TotalImport { get; set; }
        public int TotalExport { get; set; }
        public int TotalTransferIn { get; set; }
        public int TotalTransferOut { get; set; }
        public int TotalAdjustment { get; set; }
        public int CurrentStock { get; set; }
    }

    public class StockDiscrepancyItemDto
    {
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int MainStock { get; set; }
        public int SumBranchStock { get; set; }
        public int Difference => MainStock - SumBranchStock;
    }

    public interface IWarehouseAdminService
    {
        Task<(bool IsSuperAdmin, int? BranchId, string BranchName)> GetUserRoleInfoAsync(string userId);
        Task<AdminWarehouseOverviewDto> GetOverviewAsync(string userId);
        Task<(List<StockMatrixItemDto> Items, int TotalCount, List<Branch> Branches)> GetStockMatrixAsync(
            string userId, string? keyword, int? categoryId, int? branchId, bool lowStockOnly, int page, int pageSize);
        Task<List<Branch>> GetAllBranchesAsync();
        Task<Branch?> GetBranchByIdAsync(int id);
        Task<(bool Success, string Message)> SaveBranchAsync(string userId, Branch branch);
        Task<(bool Success, string Message)> ToggleBranchStatusAsync(string userId, int id);
        Task<(bool Success, string Message)> AssignBranchManagerAsync(string superAdminUserId, int branchId, string targetUserId);
        Task<(bool Success, string Message)> AdjustStockAsync(string userId, int branchId, int bookId, int newQuantity, string reason);
        Task<(bool Success, string Message)> TransferStockAsync(string userId, int fromBranchId, int toBranchId, int bookId, int quantity, string? note);
        Task<List<InventoryHistory>> GetHistoryAsync(string userId, int? branchId, string? transactionType, DateTime? fromDate, DateTime? toDate, string? keyword, int limit = 100);
        Task<List<XntReportItemDto>> GetXntReportAsync(string userId, int? branchId, DateTime? fromDate, DateTime? toDate);
        Task<List<StockDiscrepancyItemDto>> GetDiscrepanciesAsync(string userId);
        Task<(bool Success, string Message, int FixedCount)> ReconcileAllStockAsync(string userId);
    }
}

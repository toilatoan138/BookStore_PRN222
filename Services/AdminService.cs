using BookStore.Data;
using BookStore.Models.Entities;
using BookStore.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Services
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserService _userService;

        public AdminService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IUserService userService)
        {
            _context = context;
            _userManager = userManager;
            _userService = userService;
        }

        public async Task<AdminDashboardStats> GetDashboardStatsAsync()
        {
            var stats = new AdminDashboardStats();

            stats.TotalRevenue = await _context.Orders
                .Where(o => o.Status == OrderStatus.Delivered)
                .SumAsync(o => o.TotalAmount);

            stats.TotalOrders = await _context.Orders.CountAsync();
            stats.TotalBooksSold = await _context.Books.SumAsync(b => b.SoldQuantity);
            stats.TotalCustomers = await _userManager.Users.CountAsync();
            stats.PendingOrdersCount = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Pending);
            stats.LowStockBooksCount = await _context.Books.CountAsync(b => b.StockQuantity <= 5 && b.IsActive);
            stats.PendingReturnsCount = await _context.ReturnRequests.CountAsync(r => r.Status == 0);
            stats.PendingPOCount = await _context.PurchaseOrders.CountAsync(po => po.Status == 0);

            stats.RecentOrders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            stats.TopSellingBooks = await _context.Books
                .Include(b => b.Category)
                .OrderByDescending(b => b.SoldQuantity)
                .Take(5)
                .ToListAsync();

            return stats;
        }

        public async Task<List<UserManagementItem>> GetAllUsersAsync(string? keyword = null)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string kw = keyword.Trim().ToLower();
                query = query.Where(u => (u.UserName != null && u.UserName.ToLower().Contains(kw)) ||
                                         (u.Email != null && u.Email.ToLower().Contains(kw)) ||
                                         (u.FullName != null && u.FullName.ToLower().Contains(kw)));
            }

            var users = await query.OrderByDescending(u => u.CreatedAt).ToListAsync();
            var list = new List<UserManagementItem>();

            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                list.Add(new UserManagementItem
                {
                    Id = u.Id,
                    UserName = u.UserName ?? "",
                    FullName = u.FullName,
                    Email = u.Email ?? "",
                    PhoneNumber = u.PhoneNumber,
                    Status = u.Status,
                    TotalSpend = u.TotalSpend,
                    FPoints = u.FPoints,
                    WalletBalance = u.WalletBalance,
                    Role = roles.FirstOrDefault() ?? "Customer",
                    CreatedAt = u.CreatedAt
                });
            }

            return list;
        }

        public async Task<bool> ToggleUserStatusAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            user.Status = !user.Status;
            await _userManager.UpdateAsync(user);
            return true;
        }

        public async Task<bool> SetUserRoleAsync(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, newRole);

            return true;
        }

        public async Task<List<Order>> GetAllOrdersAsync(OrderStatus? status = null, string? keyword = null)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.Details)
                    .ThenInclude(d => d.Book)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string kw = keyword.Trim().ToLower();
                query = query.Where(o => o.Id.ToString().Contains(kw) ||
                                         (o.FullName != null && o.FullName.ToLower().Contains(kw)) ||
                                         (o.PhoneNumber != null && o.PhoneNumber.Contains(kw)));
            }

            return await query.OrderByDescending(o => o.OrderDate).ToListAsync();
        }

        public async Task<List<PurchaseOrder>> GetPurchaseOrdersAsync(int? status = null)
        {
            var query = _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.CreatedBy)
                .Include(po => po.ApprovedBy)
                .Include(po => po.Details)
                    .ThenInclude(d => d.Book)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(po => po.Status == status.Value);
            }

            return await query.OrderByDescending(po => po.OrderDate).ToListAsync();
        }

        public async Task<bool> ApprovePurchaseOrderAsync(int poId, string adminUserId, string? note = null)
        {
            var po = await _context.PurchaseOrders.FindAsync(poId);
            if (po == null || po.Status != 0) return false;

            po.Status = 1; // Approved
            po.ApprovedById = adminUserId;
            if (!string.IsNullOrEmpty(note)) po.StatusNote = note;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelPurchaseOrderAsync(int poId, string adminUserId, string reason)
        {
            var po = await _context.PurchaseOrders.FindAsync(poId);
            if (po == null || po.Status != 0) return false;

            po.Status = 3; // Cancelled
            po.ApprovedById = adminUserId;
            po.StatusNote = $"Admin từ chối: {reason}";

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ReturnRequest>> GetReturnRequestsAsync(int? status = null)
        {
            var query = _context.ReturnRequests
                .Include(r => r.Book)
                .Include(r => r.Order)
                    .ThenInclude(o => o.User)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            return await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
        }

        public async Task<bool> ReviewReturnRequestAsync(int returnId, int status, string? adminNote, decimal? refundAmount = null)
        {
            var req = await _context.ReturnRequests
                .Include(r => r.Order)
                .FirstOrDefaultAsync(r => r.ReturnId == returnId);

            if (req == null) return false;

            req.Status = status;
            req.AdminNote = adminNote;

            // If Approved (1) or Completed (3) and refundAmount provided -> refund to user's wallet
            if ((status == 1 || status == 3) && refundAmount.HasValue && refundAmount.Value > 0 && !string.IsNullOrEmpty(req.Order?.UserId))
            {
                await _userService.UpdateWalletBalanceAsync(
                    req.Order.UserId,
                    refundAmount.Value,
                    "REFUND",
                    $"Hoàn tiền yêu cầu trả hàng #{req.ReturnId} (Đơn #{req.OrderId})",
                    req.OrderId
                );

                _context.RefundTransactions.Add(new RefundTransaction
                {
                    ReturnId = req.ReturnId,
                    RefundAmount = refundAmount.Value,
                    BankReference = "WALLET_REFUND",
                    ProcessedBy = "Admin",
                    ProcessedAt = DateTime.UtcNow,
                    AdminNote = adminNote
                });

                // Deduct earned F-Points & TotalSpend for refunded amount
                var user = await _context.Users.FindAsync(req.Order.UserId);
                if (user != null)
                {
                    int pointsToDeduct = (int)(refundAmount.Value / 10000);
                    if (pointsToDeduct > 0)
                    {
                        user.FPoints = Math.Max(0, user.FPoints - pointsToDeduct);
                        user.TotalSpend = Math.Max(0, user.TotalSpend - refundAmount.Value);

                        _context.FPointHistories.Add(new FPointHistory
                        {
                            UserId = user.Id,
                            Amount = pointsToDeduct,
                            ActionType = "sub",
                            Reason = $"Thu hồi điểm do hoàn tiền trả hàng đơn #{req.OrderId} (Yêu cầu #{req.ReturnId})",
                            CustomerInfo = user.FullName,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Voucher>> GetAllVouchersAsync()
        {
            return await _context.Vouchers.OrderByDescending(v => v.Id).ToListAsync();
        }

        public async Task<Voucher> CreateVoucherAsync(Voucher voucher)
        {
            voucher.Code = voucher.Code.Trim().ToUpper();
            _context.Vouchers.Add(voucher);
            await _context.SaveChangesAsync();
            return voucher;
        }

        public async Task<bool> ToggleVoucherStatusAsync(int voucherId)
        {
            var voucher = await _context.Vouchers.FindAsync(voucherId);
            if (voucher == null) return false;

            voucher.Status = voucher.Status == 1 ? 0 : 1;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

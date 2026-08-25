using BookStore.Data;
using BookStore.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Services
{
    public class WarehouseAdminService : IWarehouseAdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public WarehouseAdminService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<(bool IsSuperAdmin, int? BranchId, string BranchName)> GetUserRoleInfoAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return (false, null, "Không xác định");

            bool isSuperAdmin = user.BranchId == null;
            string branchName = "Tổng Công Ty (Toàn Quốc)";

            if (!isSuperAdmin && user.BranchId.HasValue)
            {
                var branch = await _context.Branches.FindAsync(user.BranchId.Value);
                branchName = branch?.Name ?? $"Chi nhánh #{user.BranchId.Value}";
            }

            return (isSuperAdmin, user.BranchId, branchName);
        }

        public async Task<AdminWarehouseOverviewDto> GetOverviewAsync(string userId)
        {
            var (isSuperAdmin, userBranchId, userBranchName) = await GetUserRoleInfoAsync(userId);

            var overview = new AdminWarehouseOverviewDto
            {
                IsSuperAdmin = isSuperAdmin,
                UserBranchId = userBranchId,
                UserBranchName = userBranchName,
                TotalBranches = await _context.Branches.CountAsync(b => b.IsActive)
            };

            var branches = await _context.Branches.OrderBy(b => b.Id).ToListAsync();
            var allBooks = await _context.Books.Include(b => b.Category).AsNoTracking().ToListAsync();
            var allBranchInventories = await _context.BranchInventories.AsNoTracking().ToListAsync();

            if (isSuperAdmin)
            {
                overview.TotalSkus = allBooks.Count;
                overview.TotalBooksInStock = allBranchInventories.Sum(bi => bi.StockQuantity);
                
                // Tính toán giá trị tồn kho theo giá vốn & giá bán
                foreach (var book in allBooks)
                {
                    int totalStock = allBranchInventories.Where(bi => bi.BookId == book.Id).Sum(bi => bi.StockQuantity);
                    overview.TotalCostValue += totalStock * (book.ImportPrice > 0 ? book.ImportPrice : book.Price * 0.7m);
                    overview.TotalRetailValue += totalStock * book.Price;
                }

                overview.LowStockCount = allBooks.Count(b => 
                    allBranchInventories.Where(bi => bi.BookId == b.Id).Sum(bi => bi.StockQuantity) <= 5 && b.IsActive);

                overview.LowStockBooks = allBooks
                    .Where(b => allBranchInventories.Where(bi => bi.BookId == b.Id).Sum(bi => bi.StockQuantity) <= 5 && b.IsActive)
                    .OrderBy(b => allBranchInventories.Where(bi => bi.BookId == b.Id).Sum(bi => bi.StockQuantity))
                    .Take(10)
                    .ToList();

                // Lấy thông tin tóm tắt từng chi nhánh
                foreach (var branch in branches)
                {
                    var bInvs = allBranchInventories.Where(bi => bi.BranchId == branch.Id).ToList();
                    decimal costVal = 0;
                    decimal retailVal = 0;

                    foreach (var inv in bInvs)
                    {
                        var bk = allBooks.FirstOrDefault(b => b.Id == inv.BookId);
                        if (bk != null)
                        {
                            costVal += inv.StockQuantity * (bk.ImportPrice > 0 ? bk.ImportPrice : bk.Price * 0.7m);
                            retailVal += inv.StockQuantity * bk.Price;
                        }
                    }

                    // Tìm Admin quản lý chi nhánh
                    var manager = await _userManager.Users.FirstOrDefaultAsync(u => u.BranchId == branch.Id);

                    overview.BranchSummaries.Add(new BranchStockSummaryDto
                    {
                        BranchId = branch.Id,
                        BranchName = branch.Name,
                        City = branch.City,
                        Address = branch.Address,
                        IsActive = branch.IsActive,
                        TotalStock = bInvs.Sum(bi => bi.StockQuantity),
                        TotalSkus = bInvs.Count(bi => bi.StockQuantity > 0),
                        CostValue = costVal,
                        RetailValue = retailVal,
                        ManagerName = manager?.FullName ?? manager?.UserName
                    });
                }

                // Đếm số sách bị lệch tổng tồn
                overview.InconsistentBookCount = allBooks.Count(b => 
                    b.StockQuantity != allBranchInventories.Where(bi => bi.BookId == b.Id).Sum(bi => bi.StockQuantity));

                overview.RecentMovements = await _context.InventoryHistories
                    .Include(h => h.Book)
                    .Include(h => h.CreatedBy)
                    .OrderByDescending(h => h.CreatedAt)
                    .Take(15)
                    .ToListAsync();
            }
            else
            {
                // Admin Chi Nhánh: Chỉ lấy số liệu của kho mình
                int bId = userBranchId!.Value;
                var bInvs = allBranchInventories.Where(bi => bi.BranchId == bId).ToList();
                overview.TotalSkus = bInvs.Count(bi => bi.StockQuantity > 0);
                overview.TotalBooksInStock = bInvs.Sum(bi => bi.StockQuantity);

                foreach (var inv in bInvs)
                {
                    var bk = allBooks.FirstOrDefault(b => b.Id == inv.BookId);
                    if (bk != null)
                    {
                        overview.TotalCostValue += inv.StockQuantity * (bk.ImportPrice > 0 ? bk.ImportPrice : bk.Price * 0.7m);
                        overview.TotalRetailValue += inv.StockQuantity * bk.Price;
                    }
                }

                overview.LowStockCount = allBooks.Count(b => 
                {
                    int s = bInvs.FirstOrDefault(bi => bi.BookId == b.Id)?.StockQuantity ?? 0;
                    return s <= 5 && b.IsActive;
                });

                overview.LowStockBooks = allBooks
                    .Where(b => (bInvs.FirstOrDefault(bi => bi.BookId == b.Id)?.StockQuantity ?? 0) <= 5 && b.IsActive)
                    .OrderBy(b => bInvs.FirstOrDefault(bi => bi.BookId == b.Id)?.StockQuantity ?? 0)
                    .Take(10)
                    .ToList();

                var myBranch = branches.FirstOrDefault(b => b.Id == bId);
                if (myBranch != null)
                {
                    overview.BranchSummaries.Add(new BranchStockSummaryDto
                    {
                        BranchId = myBranch.Id,
                        BranchName = myBranch.Name,
                        City = myBranch.City,
                        Address = myBranch.Address,
                        IsActive = myBranch.IsActive,
                        TotalStock = overview.TotalBooksInStock,
                        TotalSkus = overview.TotalSkus,
                        CostValue = overview.TotalCostValue,
                        RetailValue = overview.TotalRetailValue
                    });
                }

                overview.RecentMovements = await _context.InventoryHistories
                    .Include(h => h.Book)
                    .Include(h => h.CreatedBy)
                    .Where(h => h.RelatedId == bId || h.CreatedById == userId)
                    .OrderByDescending(h => h.CreatedAt)
                    .Take(15)
                    .ToListAsync();
            }

            return overview;
        }

        public async Task<(List<StockMatrixItemDto> Items, int TotalCount, List<Branch> Branches)> GetStockMatrixAsync(
            string userId, string? keyword, int? categoryId, int? branchId, bool lowStockOnly, int page, int pageSize)
        {
            var (isSuperAdmin, userBranchId, _) = await GetUserRoleInfoAsync(userId);
            var branches = await _context.Branches.Where(b => b.IsActive).OrderBy(b => b.Id).ToListAsync();

            // Nếu là Admin Chi Nhánh, ép branchId về kho của mình
            if (!isSuperAdmin && userBranchId.HasValue)
            {
                branchId = userBranchId.Value;
            }

            var query = _context.Books
                .Include(b => b.Category)
                .Include(b => b.BranchInventories)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string kw = keyword.Trim().ToLower();
                query = query.Where(b => (b.Title != null && b.Title.ToLower().Contains(kw)) ||
                                         (b.Author != null && b.Author.ToLower().Contains(kw)) ||
                                         (b.Isbn != null && b.Isbn.ToLower().Contains(kw)));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(b => b.CategoryId == categoryId.Value);
            }

            if (lowStockOnly)
            {
                if (branchId.HasValue && branchId.Value > 0)
                {
                    query = query.Where(b => b.BranchInventories.Any(bi => bi.BranchId == branchId.Value && bi.StockQuantity <= 5));
                }
                else
                {
                    query = query.Where(b => b.StockQuantity <= 5);
                }
            }

            int totalCount = await query.CountAsync();
            int p = Math.Max(1, page);
            int ps = Math.Max(5, pageSize);

            var books = await query
                .OrderBy(b => b.Title)
                .Skip((p - 1) * ps)
                .Take(ps)
                .ToListAsync();

            var matrixItems = books.Select(b =>
            {
                var item = new StockMatrixItemDto
                {
                    BookId = b.Id,
                    Title = b.Title,
                    Author = b.Author,
                    ImageUrl = b.ImageUrl,
                    CategoryName = b.Category?.Name,
                    Price = b.Price,
                    ImportPrice = b.ImportPrice,
                    TotalStock = b.StockQuantity
                };

                foreach (var branch in branches)
                {
                    var inv = b.BranchInventories.FirstOrDefault(bi => bi.BranchId == branch.Id);
                    item.BranchStocks[branch.Id] = inv?.StockQuantity ?? 0;
                }

                return item;
            }).ToList();

            return (matrixItems, totalCount, branches);
        }

        public async Task<List<Branch>> GetAllBranchesAsync()
        {
            return await _context.Branches.OrderBy(b => b.Id).ToListAsync();
        }

        public async Task<Branch?> GetBranchByIdAsync(int id)
        {
            return await _context.Branches.FindAsync(id);
        }

        public async Task<(bool Success, string Message)> SaveBranchAsync(string userId, Branch branch)
        {
            var (isSuperAdmin, _, _) = await GetUserRoleInfoAsync(userId);
            if (!isSuperAdmin)
            {
                return (false, "Chỉ Super Admin mới có quyền tạo hoặc chỉnh sửa danh mục chi nhánh.");
            }

            if (string.IsNullOrWhiteSpace(branch.Name))
            {
                return (false, "Tên chi nhánh không được để trống.");
            }

            if (branch.Id > 0)
            {
                var existing = await _context.Branches.FindAsync(branch.Id);
                if (existing == null) return (false, "Không tìm thấy chi nhánh.");

                existing.Name = branch.Name.Trim();
                existing.Address = branch.Address?.Trim();
                existing.City = branch.City?.Trim();
                existing.IsActive = branch.IsActive;
            }
            else
            {
                branch.Name = branch.Name.Trim();
                branch.Address = branch.Address?.Trim();
                branch.City = branch.City?.Trim();
                _context.Branches.Add(branch);
            }

            await _context.SaveChangesAsync();
            return (true, "Đã lưu thông tin chi nhánh thành công.");
        }

        public async Task<(bool Success, string Message)> ToggleBranchStatusAsync(string userId, int id)
        {
            var (isSuperAdmin, _, _) = await GetUserRoleInfoAsync(userId);
            if (!isSuperAdmin)
            {
                return (false, "Chỉ Super Admin mới có quyền bật/tắt hoạt động chi nhánh.");
            }

            var branch = await _context.Branches.FindAsync(id);
            if (branch == null) return (false, "Không tìm thấy chi nhánh.");

            // Abnormal Case 9: Nếu đang tắt kho mà kho vẫn còn hàng tồn
            if (branch.IsActive)
            {
                int remainingStock = await _context.BranchInventories
                    .Where(bi => bi.BranchId == id)
                    .SumAsync(bi => bi.StockQuantity);

                branch.IsActive = false;
                await _context.SaveChangesAsync();

                if (remainingStock > 0)
                {
                    return (true, $"Đã tạm ngưng chi nhánh '{branch.Name}'. Lưu ý: Kho vẫn còn {remainingStock:N0} cuốn sách tồn. Khuyến nghị điều chuyển hết hàng sang kho khác!");
                }
                return (true, $"Đã tạm ngưng hoạt động chi nhánh '{branch.Name}'.");
            }
            else
            {
                branch.IsActive = true;
                await _context.SaveChangesAsync();
                return (true, $"Đã kích hoạt lại chi nhánh '{branch.Name}'.");
            }
        }

        public async Task<(bool Success, string Message)> AssignBranchManagerAsync(string superAdminUserId, int branchId, string targetUserId)
        {
            var (isSuperAdmin, _, _) = await GetUserRoleInfoAsync(superAdminUserId);
            if (!isSuperAdmin)
            {
                return (false, "Chỉ Super Admin mới có quyền bổ nhiệm Quản lý Chi nhánh.");
            }

            var branch = await _context.Branches.FindAsync(branchId);
            if (branch == null) return (false, "Không tìm thấy chi nhánh chỉ định.");

            var targetUser = await _userManager.FindByIdAsync(targetUserId);
            if (targetUser == null) return (false, "Không tìm thấy người dùng chỉ định.");

            targetUser.BranchId = branchId;
            var updateResult = await _userManager.UpdateAsync(targetUser);

            if (!updateResult.Succeeded)
            {
                return (false, "Lỗi khi cập nhật tài khoản: " + string.Join(", ", updateResult.Errors.Select(e => e.Description)));
            }

            return (true, $"Đã bổ nhiệm '{targetUser.FullName}' làm Admin quản lý chi nhánh '{branch.Name}'.");
        }

        public async Task<(bool Success, string Message)> AdjustStockAsync(
            string userId, int branchId, int bookId, int newQuantity, string reason)
        {
            var (isSuperAdmin, userBranchId, _) = await GetUserRoleInfoAsync(userId);

            // Abnormal Case 1: Unauthorized Cross-Branch
            if (!isSuperAdmin && userBranchId != branchId)
            {
                return (false, "Bạn không có quyền kiểm kê/điều chỉnh tồn kho của chi nhánh khác!");
            }

            // Abnormal Case 7: Negative stock or empty reason
            if (newQuantity < 0 || newQuantity > 1_000_000)
            {
                return (false, "Số lượng tồn kho thực tế phải >= 0 và không vượt quá 1,000,000 cuốn.");
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                return (false, "Vui lòng nhập lý do điều chỉnh kiểm kê (hư hỏng, mất mát, nhập thừa, v.v.).");
            }

            // Abnormal Case 6: Book does not exist
            var book = await _context.Books.FindAsync(bookId);
            if (book == null) return (false, "Sách không tồn tại trong hệ thống.");

            var branch = await _context.Branches.FindAsync(branchId);
            if (branch == null || !branch.IsActive) return (false, "Chi nhánh không tồn tại hoặc đang tạm ngưng hoạt động.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Abnormal Case 8: Missing BranchInventory
                var branchInv = await _context.BranchInventories
                    .FirstOrDefaultAsync(bi => bi.BranchId == branchId && bi.BookId == bookId);

                int oldQuantity = branchInv?.StockQuantity ?? 0;
                int difference = newQuantity - oldQuantity;

                if (branchInv != null)
                {
                    branchInv.StockQuantity = newQuantity;
                }
                else
                {
                    branchInv = new BranchInventory
                    {
                        BranchId = branchId,
                        BookId = bookId,
                        StockQuantity = newQuantity
                    };
                    _context.BranchInventories.Add(branchInv);
                }

                // Ghi log Inventory_History
                _context.InventoryHistories.Add(new InventoryHistory
                {
                    BookId = bookId,
                    CreatedById = userId,
                    TransactionType = "ADJUSTMENT",
                    QuantityChanged = difference,
                    RelatedId = branchId,
                    CreatedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();

                // Abnormal Case 10: Đồng bộ tổng tồn sách
                book.StockQuantity = await _context.BranchInventories
                    .Where(bi => bi.BookId == bookId)
                    .SumAsync(bi => bi.StockQuantity);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, $"Đã điều chỉnh tồn kho sách '{book.Title}' tại '{branch.Name}' từ {oldQuantity} thành {newQuantity} cuốn thành công.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, "Lỗi khi lưu dữ liệu điều chỉnh tồn kho: " + ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> TransferStockAsync(
            string userId, int fromBranchId, int toBranchId, int bookId, int quantity, string? note)
        {
            var (isSuperAdmin, userBranchId, _) = await GetUserRoleInfoAsync(userId);

            // Abnormal Case 1: Unauthorized Cross-Branch (Admin nhánh phải có kho gửi hoặc nhận thuộc kho mình)
            if (!isSuperAdmin && userBranchId.HasValue)
            {
                if (fromBranchId != userBranchId.Value && toBranchId != userBranchId.Value)
                {
                    return (false, "Bạn chỉ có thể thực hiện lệnh điều chuyển liên quan đến chi nhánh của mình.");
                }
            }

            // Abnormal Case 2: Same branch transfer
            if (fromBranchId == toBranchId)
            {
                return (false, "Kho xuất và Kho nhận không được trùng nhau.");
            }

            // Abnormal Case 4: Invalid quantity
            if (quantity <= 0 || quantity > 100_000)
            {
                return (false, "Số lượng điều chuyển phải lớn hơn 0 và không vượt quá 100,000 cuốn.");
            }

            // Abnormal Case 5: Inactive Branch
            var fromBranch = await _context.Branches.FindAsync(fromBranchId);
            var toBranch = await _context.Branches.FindAsync(toBranchId);

            if (fromBranch == null || !fromBranch.IsActive)
            {
                return (false, $"Kho xuất '{fromBranch?.Name ?? fromBranchId.ToString()}' không tồn tại hoặc đang tạm ngưng hoạt động.");
            }

            if (toBranch == null || !toBranch.IsActive)
            {
                return (false, $"Kho nhận '{toBranch?.Name ?? toBranchId.ToString()}' không tồn tại hoặc đang tạm ngưng hoạt động.");
            }

            // Abnormal Case 6: Book does not exist
            var book = await _context.Books.FindAsync(bookId);
            if (book == null) return (false, "Sách không tồn tại trong hệ thống.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Abnormal Case 3: Insufficient source stock & Concurrency check
                var sourceInv = await _context.BranchInventories
                    .FirstOrDefaultAsync(bi => bi.BranchId == fromBranchId && bi.BookId == bookId);

                int sourceStock = sourceInv?.StockQuantity ?? 0;
                if (sourceStock < quantity)
                {
                    return (false, $"Kho xuất '{fromBranch.Name}' chỉ còn {sourceStock} cuốn sách '{book.Title}', không đủ để điều chuyển {quantity} cuốn.");
                }

                // 1. Trừ kho xuất
                sourceInv!.StockQuantity -= quantity;

                // 2. Cộng kho nhận (Abnormal Case 8: Missing record handling)
                var destInv = await _context.BranchInventories
                    .FirstOrDefaultAsync(bi => bi.BranchId == toBranchId && bi.BookId == bookId);

                if (destInv == null)
                {
                    destInv = new BranchInventory
                    {
                        BranchId = toBranchId,
                        BookId = bookId,
                        StockQuantity = quantity
                    };
                    _context.BranchInventories.Add(destInv);
                }
                else
                {
                    destInv.StockQuantity += quantity;
                }

                // 3. Ghi 2 dòng log biến động
                _context.InventoryHistories.Add(new InventoryHistory
                {
                    BookId = bookId,
                    CreatedById = userId,
                    TransactionType = "TRANSFER_OUT",
                    QuantityChanged = -quantity,
                    RelatedId = toBranchId,
                    CreatedAt = DateTime.UtcNow
                });

                _context.InventoryHistories.Add(new InventoryHistory
                {
                    BookId = bookId,
                    CreatedById = userId,
                    TransactionType = "TRANSFER_IN",
                    QuantityChanged = quantity,
                    RelatedId = fromBranchId,
                    CreatedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();

                // Đồng bộ tổng tồn (tổng tồn hệ thống giữ nguyên)
                book.StockQuantity = await _context.BranchInventories
                    .Where(bi => bi.BookId == bookId)
                    .SumAsync(bi => bi.StockQuantity);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, $"Đã điều chuyển thành công {quantity} cuốn '{book.Title}' từ '{fromBranch.Name}' sang '{toBranch.Name}'.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, "Lỗi trong quá trình điều chuyển kho: " + ex.Message);
            }
        }

        public async Task<List<InventoryHistory>> GetHistoryAsync(
            string userId, int? branchId, string? transactionType, DateTime? fromDate, DateTime? toDate, string? keyword, int limit = 100)
        {
            var (isSuperAdmin, userBranchId, _) = await GetUserRoleInfoAsync(userId);
            if (!isSuperAdmin && userBranchId.HasValue)
            {
                branchId = userBranchId.Value;
            }

            var query = _context.InventoryHistories
                .Include(h => h.Book)
                .Include(h => h.CreatedBy)
                .AsNoTracking()
                .AsQueryable();

            if (branchId.HasValue && branchId.Value > 0)
            {
                query = query.Where(h => h.RelatedId == branchId.Value);
            }

            if (!string.IsNullOrWhiteSpace(transactionType))
            {
                string tt = transactionType.Trim().ToUpper();
                query = query.Where(h => h.TransactionType.ToUpper() == tt);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(h => h.CreatedAt >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                DateTime end = toDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(h => h.CreatedAt <= end);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string kw = keyword.Trim().ToLower();
                query = query.Where(h => (h.Book != null && h.Book.Title.ToLower().Contains(kw)) ||
                                         (h.CreatedBy != null && h.CreatedBy.FullName.ToLower().Contains(kw)));
            }

            return await query
                .OrderByDescending(h => h.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<XntReportItemDto>> GetXntReportAsync(string userId, int? branchId, DateTime? fromDate, DateTime? toDate)
        {
            var (isSuperAdmin, userBranchId, _) = await GetUserRoleInfoAsync(userId);
            if (!isSuperAdmin && userBranchId.HasValue)
            {
                branchId = userBranchId.Value;
            }

            var books = await _context.Books.Include(b => b.Category).Include(b => b.BranchInventories).AsNoTracking().ToListAsync();
            var historyQuery = _context.InventoryHistories.AsNoTracking().AsQueryable();

            if (branchId.HasValue && branchId.Value > 0)
            {
                historyQuery = historyQuery.Where(h => h.RelatedId == branchId.Value);
            }

            if (fromDate.HasValue)
            {
                historyQuery = historyQuery.Where(h => h.CreatedAt >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                DateTime end = toDate.Value.Date.AddDays(1).AddTicks(-1);
                historyQuery = historyQuery.Where(h => h.CreatedAt <= end);
            }

            var histories = await historyQuery.ToListAsync();

            var reportList = books.Select(b =>
            {
                var bookHistories = histories.Where(h => h.BookId == b.Id).ToList();
                int currentStock = branchId.HasValue 
                    ? (b.BranchInventories.FirstOrDefault(bi => bi.BranchId == branchId.Value)?.StockQuantity ?? 0)
                    : b.StockQuantity;

                return new XntReportItemDto
                {
                    BookId = b.Id,
                    Title = b.Title,
                    CategoryName = b.Category?.Name,
                    TotalImport = bookHistories.Where(h => h.TransactionType == "IMPORT").Sum(h => h.QuantityChanged),
                    TotalExport = Math.Abs(bookHistories.Where(h => h.TransactionType == "EXPORT").Sum(h => h.QuantityChanged)),
                    TotalTransferIn = bookHistories.Where(h => h.TransactionType == "TRANSFER_IN").Sum(h => h.QuantityChanged),
                    TotalTransferOut = Math.Abs(bookHistories.Where(h => h.TransactionType == "TRANSFER_OUT").Sum(h => h.QuantityChanged)),
                    TotalAdjustment = bookHistories.Where(h => h.TransactionType == "ADJUSTMENT").Sum(h => h.QuantityChanged),
                    CurrentStock = currentStock
                };
            }).ToList();

            return reportList;
        }

        public async Task<List<StockDiscrepancyItemDto>> GetDiscrepanciesAsync(string userId)
        {
            var (isSuperAdmin, _, _) = await GetUserRoleInfoAsync(userId);
            if (!isSuperAdmin) return new List<StockDiscrepancyItemDto>();

            var allBooks = await _context.Books.AsNoTracking().ToListAsync();
            var allBranchInventories = await _context.BranchInventories.AsNoTracking().ToListAsync();

            var discrepancies = new List<StockDiscrepancyItemDto>();

            foreach (var b in allBooks)
            {
                int sumBranch = allBranchInventories.Where(bi => bi.BookId == b.Id).Sum(bi => bi.StockQuantity);
                if (b.StockQuantity != sumBranch)
                {
                    discrepancies.Add(new StockDiscrepancyItemDto
                    {
                        BookId = b.Id,
                        Title = b.Title,
                        MainStock = b.StockQuantity,
                        SumBranchStock = sumBranch
                    });
                }
            }

            return discrepancies;
        }

        public async Task<(bool Success, string Message, int FixedCount)> ReconcileAllStockAsync(string userId)
        {
            var (isSuperAdmin, _, _) = await GetUserRoleInfoAsync(userId);
            if (!isSuperAdmin)
            {
                return (false, "Chỉ Super Admin mới có quyền chạy tính năng Tự động Đồng bộ Tồn kho Toàn diện.", 0);
            }

            var books = await _context.Books.ToListAsync();
            var allBranchInventories = await _context.BranchInventories.ToListAsync();
            int fixedCount = 0;

            foreach (var b in books)
            {
                int sumBranch = allBranchInventories.Where(bi => bi.BookId == b.Id).Sum(bi => bi.StockQuantity);
                if (b.StockQuantity != sumBranch)
                {
                    b.StockQuantity = sumBranch;
                    fixedCount++;
                }
            }

            if (fixedCount > 0)
            {
                await _context.SaveChangesAsync();
            }

            return (true, $"Đã kiểm tra và tự động đồng bộ lại số lượng tồn kho cho {fixedCount} đầu sách thành công.", fixedCount);
        }
    }
}

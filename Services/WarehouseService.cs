using BookStore.Data;
using BookStore.Models.Entities;
using BookStore.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly ApplicationDbContext _context;

        public WarehouseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<WarehouseDashboardStats> GetDashboardStatsAsync()
        {
            var stats = new WarehouseDashboardStats
            {
                TotalBooksInStock = await _context.Books.SumAsync(b => b.StockQuantity),
                TotalSkus = await _context.Books.CountAsync(),
                LowStockCount = await _context.Books.CountAsync(b => b.StockQuantity <= 5 && b.IsActive),
                PendingReceivingPOCount = await _context.PurchaseOrders.CountAsync(po => po.Status == 1), // Approved by Admin, waiting for arrival
                PendingPickingOrdersCount = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Processing),
                LowStockBooks = await _context.Books
                    .Include(b => b.Category)
                    .Where(b => b.StockQuantity <= 5 && b.IsActive)
                    .OrderBy(b => b.StockQuantity)
                    .Take(5)
                    .ToListAsync(),
                PendingReceivingPOs = await _context.PurchaseOrders
                    .Include(po => po.Supplier)
                    .Where(po => po.Status == 1)
                    .OrderByDescending(po => po.OrderDate)
                    .Take(5)
                    .ToListAsync()
            };

            return stats;
        }

        public async Task<List<Book>> GetInventoryAsync(string? keyword = null, bool lowStockOnly = false)
        {
            var query = _context.Books
                .Include(b => b.Category)
                .Include(b => b.Location)
                .AsQueryable();

            if (lowStockOnly)
            {
                query = query.Where(b => b.StockQuantity <= 5 && b.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string kw = keyword.Trim().ToLower();
                query = query.Where(b => (b.Title != null && b.Title.ToLower().Contains(kw)) ||
                                         (b.Author != null && b.Author.ToLower().Contains(kw)) ||
                                         (b.Location != null && b.Location.LocationCode != null && b.Location.LocationCode.ToLower().Contains(kw)));
            }

            return await query.OrderBy(b => b.StockQuantity).ToListAsync();
        }

        public async Task<bool> AdjustStockAsync(int branchId, int bookId, int newQuantity, string? reason, string userId, string? locationCode = null)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null) return false;

            var branchInv = await _context.BranchInventories
                .FirstOrDefaultAsync(bi => bi.BranchId == branchId && bi.BookId == bookId);

            int oldQuantity = branchInv?.StockQuantity ?? 0;
            int changeAmount = newQuantity - oldQuantity;

            if (branchInv != null)
            {
                branchInv.StockQuantity = newQuantity;
            }
            else
            {
                _context.BranchInventories.Add(new BranchInventory
                {
                    BranchId = branchId,
                    BookId = bookId,
                    StockQuantity = newQuantity
                });
            }

            if (!string.IsNullOrEmpty(locationCode))
            {
                string formattedCode = locationCode.Trim().ToUpper();
                var loc = await _context.Locations.FirstOrDefaultAsync(l => l.LocationCode == formattedCode);
                if (loc != null)
                {
                    book.LocationId = loc.Id;
                }
            }

            // Log history
            _context.InventoryHistories.Add(new InventoryHistory
            {
                BookId = bookId,
                CreatedById = userId,
                TransactionType = "ADJUSTMENT",
                QuantityChanged = changeAmount,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Location>> GetLocationsAsync()
        {
            return await _context.Locations.OrderBy(l => l.LocationCode).ToListAsync();
        }

        public async Task<Location> CreateLocationAsync(Location location)
        {
            // location_code là computed column (zone-rack-shelf), DB tự tính — không cần gán tay
            _context.Locations.Add(location);
            await _context.SaveChangesAsync();
            return location;
        }

        public async Task<List<Supplier>> GetSuppliersAsync()
        {
            return await _context.Suppliers.OrderBy(s => s.Name).ToListAsync();
        }

        public async Task<Supplier> CreateSupplierAsync(Supplier supplier)
        {
            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();
            return supplier;
        }

        public async Task<Supplier?> GetSupplierByIdAsync(int id)
        {
            return await _context.Suppliers.FindAsync(id);
        }

        public async Task<bool> UpdateSupplierAsync(Supplier supplier)
        {
            var existing = await _context.Suppliers.FindAsync(supplier.Id);
            if (existing == null) return false;

            existing.Name = supplier.Name;
            existing.ContactPerson = supplier.ContactPerson;
            existing.Phone = supplier.Phone;
            existing.Email = supplier.Email;
            existing.Address = supplier.Address;

            await _context.SaveChangesAsync();
            return true;
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

        public async Task<PurchaseOrder?> GetPurchaseOrderByIdAsync(int id)
        {
            return await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.CreatedBy)
                .Include(po => po.ApprovedBy)
                .Include(po => po.Details)
                    .ThenInclude(d => d.Book)
                .FirstOrDefaultAsync(po => po.PurchaseOrderId == id);
        }

        public async Task<List<PurchaseOrder>> CreatePurchaseOrdersAsync(string userId, List<PoItemInput> items, int branchId)
        {
            var createdPos = new List<PurchaseOrder>();
            var groupedItems = items.GroupBy(i => i.SupplierId).ToList();

            foreach (var group in groupedItems)
            {
                int supplierId = group.Key;
                var groupItems = group.ToList();

                var po = new PurchaseOrder
                {
                    SupplierId = supplierId,
                    BranchId = branchId,
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    Status = 0, // Pending Admin Approval
                    TotalQuantity = groupItems.Sum(i => i.Quantity),
                    TotalAmount = groupItems.Sum(i => i.Quantity * i.UnitPrice)
                };

                _context.PurchaseOrders.Add(po);
                await _context.SaveChangesAsync();

                foreach (var item in groupItems)
                {
                    _context.PurchaseOrderDetails.Add(new PurchaseOrderDetail
                    {
                        PurchaseOrderId = po.PurchaseOrderId,
                        BookId = item.BookId,
                        ExpectedQuantity = item.Quantity,
                        ReceivedQuantity = 0,
                        Price = item.UnitPrice
                    });
                }

                await _context.SaveChangesAsync();
                createdPos.Add(po);
            }
            
            return createdPos;
        }

        public async Task<bool> ReceivePurchaseOrderGoodsAsync(int poId, string warehouseUserId)
        {
            var po = await _context.PurchaseOrders
                .Include(p => p.Details)
                .FirstOrDefaultAsync(p => p.PurchaseOrderId == poId);

            if (po == null || po.Status != 1) return false; // Must be Approved by Admin

            po.Status = 2; // Received
            po.StatusNote = $"Đã nhập kho thành công bởi thủ kho lúc {DateTime.UtcNow:dd/MM/yyyy HH:mm}";

            // Update Stock of each book in BranchInventory & log movement
            foreach (var d in po.Details)
            {
                d.ReceivedQuantity = d.ExpectedQuantity;

                var book = await _context.Books.FindAsync(d.BookId);
                if (book != null)
                {
                    book.ImportPrice = d.Price; // Update latest cost price
                    
                    var branchInv = await _context.BranchInventories
                        .FirstOrDefaultAsync(bi => bi.BranchId == po.BranchId && bi.BookId == d.BookId);
                        
                    if (branchInv != null)
                    {
                        branchInv.StockQuantity += d.ExpectedQuantity;
                    }
                    else
                    {
                        _context.BranchInventories.Add(new BranchInventory
                        {
                            BranchId = po.BranchId,
                            BookId = d.BookId,
                            StockQuantity = d.ExpectedQuantity
                        });
                    }

                    _context.InventoryHistories.Add(new InventoryHistory
                    {
                        BookId = d.BookId,
                        CreatedById = warehouseUserId,
                        TransactionType = "IMPORT",
                        QuantityChanged = d.ExpectedQuantity,
                        RelatedId = po.PurchaseOrderId,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            // Create Invoice
            _context.Invoices.Add(new Invoice
            {
                InvoiceType = "PURCHASE",
                PurchaseOrderId = po.PurchaseOrderId,
                TotalAmount = po.TotalAmount,
                Status = "Paid",
                CreatedDate = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Order>> GetPickingListAsync()
        {
            return await _context.Orders
                .Include(o => o.Details)
                    .ThenInclude(d => d.Book)
                .Where(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Processing)
                .OrderBy(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<List<Invoice>> GetInvoicesAsync()
        {
            return await _context.Invoices
                .Include(i => i.Order)
                .Include(i => i.PurchaseOrder)
                .OrderByDescending(i => i.CreatedDate)
                .ToListAsync();
        }
    }
}

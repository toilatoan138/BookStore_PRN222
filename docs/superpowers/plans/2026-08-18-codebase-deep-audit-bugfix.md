# Codebase Deep Audit & Bug Fix Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Đọc, kiểm toán toàn bộ codebase `BookStoreRazor`, phân tích tất cả các luồng nghiệp vụ (Customer, Staff, Warehouse, Admin), phát hiện các lỗi logic / bảo mật / thiếu sót dữ liệu chứng từ và tiến hành sửa chữa triệt để.

**Architecture:** ASP.NET Core 8/9 Razor Pages + Entity Framework Core (SQL Server) + ASP.NET Identity + SignalR Chat Hub + VNPay Integration + Bootstrap 5.

**Tech Stack:** C#, .NET 8/9, EF Core, ASP.NET Core Identity, SignalR, Razor Pages, Bootstrap 5, FontAwesome.

---

## File Structure & Impact Map

### Core Configuration & Services
- Modify: `Program.cs` - Fix `RequireWarehouseRole` policy to allow Admin access.
- Modify: `Services/OrderService.cs` - Fix missing SALE Invoice creation, missing EXPORT InventoryHistory logging, initial order status for VNPAY/WALLET, and stock cancellation logging.
- Modify: `Services/BookService.cs` - Fix Flash Sale querying to join with active `PromotionBooks` and apply real promo discounts.
- Modify: `Services/WarehouseService.cs` - Ensure Purchase Invoices & PO receiving are consistent.
- Modify: `Services/VnPayService.cs` - Ensure transaction callback robustness.

### Customer Pages & Workflows
- Modify: `Pages/Checkout/Index.cshtml.cs` & `Pages/Checkout/VnPayReturn.cshtml.cs` - Ensure session keys & order creation handle all edge cases.
- Modify: `Pages/Account/VerifyOtp.cshtml.cs` - Ensure robust session fallback for OTP registration.
- Modify: `Pages/Products/Detail.cshtml.cs` & `Pages/Products/FlashSale.cshtml.cs` - Ensure promo discount display consistency.
- Modify: `Pages/Orders/Detail.cshtml.cs` - Ensure Return request and Cancel order are strictly checked.

### Warehouse & Staff Workflows
- Modify: `Pages/Warehouse/Invoices/Index.cshtml` & `Index.cshtml.cs` - Ensure SALE & PURCHASE invoices load cleanly.
- Modify: `Pages/Warehouse/Inventory/History.cshtml.cs` - Ensure all transaction types (`IMPORT`, `EXPORT`, `ADJUSTMENT`, `RETURN`) are displayed.
- Modify: `Pages/Staff/Tickets/Index.cshtml.cs` - Ensure ticket resolution and return approval sync smoothly with Warehouse.

---

## Tasks Decomposition

### Task 1: Fix Security, Authorization Policies & Configuration

**Files:**
- Modify: `Program.cs:110-120`

- [ ] **Step 1: Inspect and fix Authorization Policies in Program.cs**
  Update `RequireWarehouseRole` to allow both `Warehouse` and `Admin` roles.

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireStaffRole", policy => policy.RequireRole("Staff", "Admin"));
    options.AddPolicy("RequireWarehouseRole", policy => policy.RequireRole("Warehouse", "Admin"));
});
```

- [ ] **Step 2: Verify compilation**
  Run: `dotnet build f:\PRN222_Project\BookStoreRazor\BookStoreRazor.csproj`
  Expected: Build succeeded.

---

### Task 2: Fix Order Placement Lifecycle (SALE Invoice & EXPORT Inventory History)

**Files:**
- Modify: `Services/OrderService.cs`

- [ ] **Step 1: Update CreateOrderAsync in OrderService.cs**
  - For online payments (`WALLET`, `VNPAY`), set initial order status to `OrderStatus.Processing` (Đã thanh toán / Chờ soạn kho).
  - Automatically create a `SALE` `Invoice` in `_context.Invoices` upon successful order creation.
  - Automatically create `InventoryHistory` records with `TransactionType = "EXPORT"` and `QuantityChanged = -item.Quantity` for each purchased book.

```csharp
// 1. Initial status
var initialStatus = OrderStatus.Pending;
if (request.PaymentMethod == "WALLET" || request.PaymentMethod == "VNPAY")
{
    initialStatus = OrderStatus.Processing;
}
else if (request.PaymentMethod == "COD" && totalQty <= 10 && calc.FinalTotal <= 2000000)
{
    initialStatus = OrderStatus.Processing;
}

// 2. Inventory History & Stock
foreach (var item in cartItems)
{
    order.Details.Add(new OrderDetail
    {
        BookId = item.BookId,
        Quantity = item.Quantity,
        Price = item.Book.Price
    });

    item.Book.StockQuantity -= item.Quantity;
    item.Book.SoldQuantity += item.Quantity;

    _context.InventoryHistories.Add(new InventoryHistory
    {
        BookId = item.BookId,
        CreatedById = request.UserId,
        TransactionType = "EXPORT",
        QuantityChanged = -item.Quantity,
        RelatedId = order.Id,
        CreatedAt = DateTime.UtcNow
    });
}

// 3. SALE Invoice
_context.Invoices.Add(new Invoice
{
    InvoiceType = "SALE",
    OrderId = order.Id,
    TotalAmount = order.TotalAmount,
    Status = (request.PaymentMethod == "WALLET" || request.PaymentMethod == "VNPAY") ? "Paid" : "Pending",
    CreatedDate = DateTime.UtcNow
});
```

- [ ] **Step 2: Update CancelOrderAsync in OrderService.cs**
  - When cancelling an order, log `InventoryHistory` with `TransactionType = "ADJUSTMENT"` or `RETURN` to track returned stock.

- [ ] **Step 3: Verify compilation**
  Run: `dotnet build f:\PRN222_Project\BookStoreRazor\BookStoreRazor.csproj`
  Expected: Build succeeded.

---

### Task 3: Fix Flash Sale & Promotion Querying Logic

**Files:**
- Modify: `Services/BookService.cs:100-115`
- Modify: `Pages/Products/FlashSale.cshtml.cs`

- [ ] **Step 1: Update GetFlashSaleBooksAsync in BookService.cs**
  Query books currently participating in active `Promotions` (`p.IsActive && p.StartDate <= now && p.EndDate >= now`). If none exist, fall back to top selling books.

```csharp
public async Task<List<Book>> GetFlashSaleBooksAsync(int limit = 10)
{
    var now = DateTime.UtcNow;
    var promoBookIds = await _context.PromotionBooks
        .Where(pb => pb.Promotion.IsActive && pb.Promotion.StartDate <= now && pb.Promotion.EndDate >= now)
        .Select(pb => pb.BookId)
        .Distinct()
        .ToListAsync();

    if (promoBookIds.Any())
    {
        return await _context.Books
            .Include(b => b.Category)
            .Where(b => b.IsActive && promoBookIds.Contains(b.Id) && b.StockQuantity > 0)
            .OrderByDescending(b => b.SoldQuantity)
            .Take(limit)
            .ToListAsync();
    }

    return await _context.Books
        .Include(b => b.Category)
        .Where(b => b.IsActive && b.StockQuantity > 0)
        .OrderByDescending(b => b.SoldQuantity)
        .Take(limit)
        .ToListAsync();
}
```

- [ ] **Step 2: Verify compilation**
  Run: `dotnet build f:\PRN222_Project\BookStoreRazor\BookStoreRazor.csproj`
  Expected: Build succeeded.

---

### Task 4: Fix OTP Registration & Session Expiry Resilience

**Files:**
- Modify: `Pages/Account/VerifyOtp.cshtml.cs`

- [ ] **Step 1: Add null safety checks for session variables in VerifyOtp.cshtml.cs**
  Ensure safe handling if session fields are missing, preventing `NullReferenceException` during user creation.

- [ ] **Step 2: Verify compilation**
  Run: `dotnet build f:\PRN222_Project\BookStoreRazor\BookStoreRazor.csproj`
  Expected: Build succeeded.

---

### Task 5: End-to-End Build, Verification & Regression Testing

**Files:**
- Verify: Full project build & runtime check

- [ ] **Step 1: Execute clean build**
  Run: `dotnet build f:\PRN222_Project\BookStoreRazor\BookStoreRazor.csproj`
  Expected: 0 Warnings, 0 Errors.

- [ ] **Step 2: Create comprehensive walkthrough artifact**
  Document all fixed bugs and verify test results.

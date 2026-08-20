using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BookStore.Models.Entities;

namespace BookStore.Data
{
    /// <summary>
    /// ApplicationDbContext — EF Core DbContext tích hợp ASP.NET Core Identity
    /// và map toàn bộ bảng nghiệp vụ từ script.sql (dbo.Books, dbo.Categories, etc.)
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // --- DbSet cho tất cả Entity nghiệp vụ ---
        public DbSet<Book> Books => Set<Book>();
        public DbSet<BookImage> BookImages => Set<BookImage>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();
        public DbSet<Address> Addresses => Set<Address>();
        public DbSet<Cart> Carts => Set<Cart>();
        public DbSet<CartItem> CartItems => Set<CartItem>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<ReportedReview> ReportedReviews => Set<ReportedReview>();
        public DbSet<Voucher> Vouchers => Set<Voucher>();
        public DbSet<UserVoucher> UserVouchers => Set<UserVoucher>();
        public DbSet<Collection> Collections => Set<Collection>();
        public DbSet<CollectionBook> CollectionBooks => Set<CollectionBook>();
        public DbSet<Promotion> Promotions => Set<Promotion>();
        public DbSet<PromotionBook> PromotionBooks => Set<PromotionBook>();
        public DbSet<Supplier> Suppliers => Set<Supplier>();
        public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
        public DbSet<PurchaseOrderDetail> PurchaseOrderDetails => Set<PurchaseOrderDetail>();
        public DbSet<Location> Locations => Set<Location>();
        public DbSet<InventoryHistory> InventoryHistories => Set<InventoryHistory>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<AdminNotification> AdminNotifications => Set<AdminNotification>();
        public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
        public DbSet<ReturnRequest> ReturnRequests => Set<ReturnRequest>();
        public DbSet<RefundTransaction> RefundTransactions => Set<RefundTransaction>();
        public DbSet<WalletHistory> WalletHistories => Set<WalletHistory>();
        public DbSet<FPointHistory> FPointHistories => Set<FPointHistory>();
        public DbSet<CustomerNote> CustomerNotes => Set<CustomerNote>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // Identity tables config

            // script.sql dùng kiểu [datetime] (không phải [datetime2] mặc định của EF Core)
            // cho toàn bộ cột ngày giờ — áp dụng quy ước chung cho mọi property DateTime.
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                    {
                        property.SetColumnType("datetime");
                    }
                }
            }

            // ====================================================================
            // CATEGORY — Self-referencing (Danh mục cha-con)
            // ====================================================================
            builder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories");
                entity.HasIndex(c => c.Name);

                entity.HasOne(c => c.Parent)
                      .WithMany(c => c.Children)
                      .HasForeignKey(c => c.ParentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ====================================================================
            // BOOK
            // ====================================================================
            builder.Entity<Book>(entity =>
            {
                entity.ToTable("Books");
                entity.HasIndex(b => b.Title);
                entity.HasIndex(b => b.Isbn).IsUnique().HasFilter("[ISBN] IS NOT NULL");
                entity.HasIndex(b => b.IsActive);

                entity.HasOne(b => b.Category)
                      .WithMany(c => c.Books)
                      .HasForeignKey(b => b.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(b => b.Location)
                      .WithMany(l => l.Books)
                      .HasForeignKey(b => b.LocationId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(b => b.SupplierEntity)
                      .WithMany(s => s.Books)
                      .HasForeignKey(b => b.SupplierId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ====================================================================
            // BOOK IMAGE — 1-N: Book → BookImages
            // ====================================================================
            builder.Entity<BookImage>(entity =>
            {
                entity.ToTable("BookImages");
                entity.HasOne(bi => bi.Book)
                      .WithMany(b => b.DetailImages)
                      .HasForeignKey(bi => bi.BookId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ====================================================================
            // ORDER — 1-N: User → Orders
            // ====================================================================
            builder.Entity<Order>(entity =>
            {
                entity.ToTable("Orders");
                entity.HasIndex(o => o.UserId);
                entity.HasIndex(o => o.OrderDate);
                entity.HasIndex(o => o.Status);

                entity.HasOne(o => o.User)
                      .WithMany(u => u.Orders)
                      .HasForeignKey(o => o.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(o => o.Voucher)
                      .WithMany(v => v.Orders)
                      .HasForeignKey(o => o.VoucherId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ====================================================================
            // ORDER DETAIL — 1-N: Order → OrderDetails
            // ====================================================================
            builder.Entity<OrderDetail>(entity =>
            {
                entity.ToTable("OrderDetails");
                entity.HasOne(od => od.Order)
                      .WithMany(o => o.Details)
                      .HasForeignKey(od => od.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(od => od.Book)
                      .WithMany(b => b.OrderDetails)
                      .HasForeignKey(od => od.BookId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ====================================================================
            // ADDRESS — 1-N: User → Addresses
            // ====================================================================
            builder.Entity<Address>(entity =>
            {
                entity.ToTable("Addresses");
                entity.HasOne(a => a.User)
                      .WithMany(u => u.Addresses)
                      .HasForeignKey(a => a.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ====================================================================
            // CART & CART ITEM — 1-N: User → Cart → CartItems, Book → CartItems
            // ====================================================================
            builder.Entity<Cart>(entity =>
            {
                entity.ToTable("Cart");
                entity.HasKey(c => c.CartId);

                entity.HasOne(c => c.User)
                      .WithMany(u => u.Carts)
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<CartItem>(entity =>
            {
                entity.ToTable("CartItems");
                entity.HasKey(ci => ci.Id);
                entity.HasIndex(ci => new { ci.CartId, ci.BookId }).IsUnique();

                entity.HasOne(ci => ci.Cart)
                      .WithMany(c => c.Items)
                      .HasForeignKey(ci => ci.CartId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ci => ci.Book)
                      .WithMany()
                      .HasForeignKey(ci => ci.BookId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ====================================================================
            // REVIEW — 1-N: User → Reviews, Book → Reviews
            // ====================================================================
            builder.Entity<Review>(entity =>
            {
                entity.ToTable("Review");
                entity.HasIndex(r => new { r.UserId, r.BookId });

                entity.HasOne(r => r.User)
                      .WithMany(u => u.Reviews)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Book)
                      .WithMany(b => b.Reviews)
                      .HasForeignKey(r => r.BookId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ====================================================================
            // REPORTED REVIEWS
            // ====================================================================
            builder.Entity<ReportedReview>(entity =>
            {
                entity.ToTable("Reported_Reviews");
                entity.HasOne(rr => rr.Review)
                      .WithMany()
                      .HasForeignKey(rr => rr.ReviewId)
                      // Restrict (không Cascade): Review đồng thời cascade từ User,
                      // nếu ReportedReview cũng cascade từ Review thì SQL Server sẽ
                      // từ chối tạo FK vì phát sinh "multiple cascade paths" từ AspNetUsers.
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(rr => rr.User)
                      .WithMany()
                      .HasForeignKey(rr => rr.UserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ====================================================================
            // VOUCHER
            // ====================================================================
            builder.Entity<Voucher>(entity =>
            {
                entity.ToTable("Vouchers");
                entity.HasIndex(v => v.Code).IsUnique();
            });

            // ====================================================================
            // USER VOUCHER — Composite PK: (UserId, VoucherId)
            // ====================================================================
            builder.Entity<UserVoucher>(entity =>
            {
                entity.ToTable("User_Vouchers");
                entity.HasKey(uv => new { uv.UserId, uv.VoucherId });

                entity.HasOne(uv => uv.User)
                      .WithMany(u => u.UserVouchers)
                      .HasForeignKey(uv => uv.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(uv => uv.Voucher)
                      .WithMany(v => v.UserVouchers)
                      .HasForeignKey(uv => uv.VoucherId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ====================================================================
            // COLLECTION — 1-N: User → Collections
            // ====================================================================
            builder.Entity<Collection>(entity =>
            {
                entity.ToTable("Collections");
                entity.HasOne(c => c.User)
                      .WithMany(u => u.Collections)
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ====================================================================
            // COLLECTION BOOK — Composite PK: (CollectionId, BookId)
            // ====================================================================
            builder.Entity<CollectionBook>(entity =>
            {
                entity.ToTable("Collection_Books");
                entity.HasKey(cb => new { cb.CollectionId, cb.BookId });

                entity.HasOne(cb => cb.Collection)
                      .WithMany(c => c.CollectionBooks)
                      .HasForeignKey(cb => cb.CollectionId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(cb => cb.Book)
                      .WithMany(b => b.CollectionBooks)
                      .HasForeignKey(cb => cb.BookId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ====================================================================
            // PROMOTION BOOK — Composite PK: (PromoId, BookId)
            // ====================================================================
            builder.Entity<PromotionBook>(entity =>
            {
                entity.ToTable("Promotion_Books");
                entity.HasKey(pb => new { pb.PromoId, pb.BookId });

                entity.HasOne(pb => pb.Promotion)
                      .WithMany(p => p.PromotionBooks)
                      .HasForeignKey(pb => pb.PromoId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pb => pb.Book)
                      .WithMany(b => b.PromotionBooks)
                      .HasForeignKey(pb => pb.BookId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ====================================================================
            // SUPPLIER
            // ====================================================================
            builder.Entity<Supplier>(entity =>
            {
                entity.ToTable("Suppliers");
                entity.HasIndex(s => s.Name);
            });

            // ====================================================================
            // PURCHASE ORDER — FK: Supplier, CreatedBy(User), ApprovedBy(User)
            // ====================================================================
            builder.Entity<PurchaseOrder>(entity =>
            {
                entity.ToTable("Purchase_Orders");
                entity.HasIndex(po => po.Status);

                entity.HasOne(po => po.Supplier)
                      .WithMany(s => s.PurchaseOrders)
                      .HasForeignKey(po => po.SupplierId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(po => po.CreatedBy)
                      .WithMany()
                      .HasForeignKey(po => po.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(po => po.ApprovedBy)
                      .WithMany()
                      .HasForeignKey(po => po.ApprovedById)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ====================================================================
            // PURCHASE ORDER DETAIL
            // ====================================================================
            builder.Entity<PurchaseOrderDetail>(entity =>
            {
                entity.ToTable("Purchase_Order_Details");
                entity.HasOne(pod => pod.PurchaseOrder)
                      .WithMany(po => po.Details)
                      .HasForeignKey(pod => pod.PurchaseOrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pod => pod.Book)
                      .WithMany(b => b.PurchaseOrderDetails)
                      .HasForeignKey(pod => pod.BookId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ====================================================================
            // LOCATION — FK optional: Category
            // ====================================================================
            builder.Entity<Location>(entity =>
            {
                entity.ToTable("Warehouse_Locations");

                // Cột computed persisted giống hệt script.sql: zone + '-' + rack + '-' + shelf
                // (luôn NOT NULL vì zone/rack/shelf đều NOT NULL) — không dùng HasFilter vì
                // SQL Server không cho phép filtered index tham chiếu computed column.
                entity.Property(l => l.LocationCode)
                      .HasComputedColumnSql("(((([zone]+'-')+[rack])+'-')+[shelf])", stored: true);
                entity.HasIndex(l => l.LocationCode).IsUnique();

                entity.HasOne(l => l.Category)
                      .WithMany(c => c.Locations)
                      .HasForeignKey(l => l.CategoryId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ====================================================================
            // INVENTORY HISTORY
            // ====================================================================
            builder.Entity<InventoryHistory>(entity =>
            {
                entity.ToTable("Inventory_History");
                entity.HasIndex(ih => ih.BookId);
                entity.HasIndex(ih => ih.CreatedAt);

                entity.HasOne(ih => ih.Book)
                      .WithMany(b => b.InventoryHistories)
                      .HasForeignKey(ih => ih.BookId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ih => ih.CreatedBy)
                      .WithMany()
                      .HasForeignKey(ih => ih.CreatedById)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ====================================================================
            // INVOICE — FK optional: Order, PurchaseOrder
            // ====================================================================
            builder.Entity<Invoice>(entity =>
            {
                entity.ToTable("Invoices");
                entity.HasIndex(i => i.InvoiceType);

                entity.HasOne(i => i.Order)
                      .WithMany()
                      .HasForeignKey(i => i.OrderId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(i => i.PurchaseOrder)
                      .WithMany()
                      .HasForeignKey(i => i.PurchaseOrderId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ====================================================================
            // NOTIFICATION — 1-N: User → Notifications
            // ====================================================================
            builder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notifications");
                entity.HasIndex(n => n.UserId);

                entity.HasOne(n => n.User)
                      .WithMany(u => u.Notifications)
                      .HasForeignKey(n => n.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ====================================================================
            // ADMIN NOTIFICATION
            // ====================================================================
            builder.Entity<AdminNotification>(entity =>
            {
                entity.ToTable("AdminNotifications");
            });

            // ====================================================================
            // SUPPORT TICKET — 1-N: User → SupportTickets
            // ====================================================================
            builder.Entity<SupportTicket>(entity =>
            {
                entity.ToTable("Support_Tickets");
                entity.HasOne(st => st.User)
                      .WithMany(u => u.SupportTickets)
                      .HasForeignKey(st => st.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ====================================================================
            // RETURN REQUEST & REFUND TRANSACTIONS
            // ====================================================================
            builder.Entity<ReturnRequest>(entity =>
            {
                entity.ToTable("ReturnRequests");
                entity.HasOne(rr => rr.Order)
                      .WithMany(o => o.ReturnRequests)
                      .HasForeignKey(rr => rr.OrderId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(rr => rr.Book)
                      .WithMany()
                      .HasForeignKey(rr => rr.BookId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<RefundTransaction>(entity =>
            {
                entity.ToTable("RefundTransactions");
                entity.HasOne(rt => rt.ReturnRequest)
                      .WithMany(rr => rr.RefundTransactions)
                      .HasForeignKey(rt => rt.ReturnId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ====================================================================
            // WALLET HISTORY — 1-N: User → WalletHistories
            // ====================================================================
            builder.Entity<WalletHistory>(entity =>
            {
                entity.ToTable("Wallet_History");
                entity.HasOne(wh => wh.User)
                      .WithMany(u => u.WalletHistories)
                      .HasForeignKey(wh => wh.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(wh => wh.Order)
                      .WithMany()
                      .HasForeignKey(wh => wh.OrderId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ====================================================================
            // F-POINT HISTORY — 1-N: User → FPointHistories
            // ====================================================================
            builder.Entity<FPointHistory>(entity =>
            {
                entity.ToTable("FPoint_History");
                entity.HasOne(fph => fph.User)
                      .WithMany(u => u.FPointHistories)
                      .HasForeignKey(fph => fph.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ====================================================================
            // CUSTOMER NOTE — 1-N: User → CustomerNotes
            // ====================================================================
            builder.Entity<CustomerNote>(entity =>
            {
                entity.ToTable("Customer_Notes");

                // script.sql khai báo follow_up_date là [date] (không phải [datetime])
                entity.Property(cn => cn.FollowUpDate).HasColumnType("date");

                entity.HasOne(cn => cn.User)
                      .WithMany(u => u.CustomerNotes)
                      .HasForeignKey(cn => cn.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}

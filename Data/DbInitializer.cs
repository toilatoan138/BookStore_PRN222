using BookStore.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger logger)
        {
            // 1. Áp dụng migrations tự động
            try
            {
                await context.Database.MigrateAsync();
            }
            catch (Microsoft.Data.SqlClient.SqlException sqlEx) when (sqlEx.Number == 2714) // 2714 = Object already exists
            {
                logger.LogWarning("Một số bảng đã tồn tại trong database từ trước. Đang đăng ký baseline migration vào __EFMigrationsHistory...");
                try
                {
                    await context.Database.ExecuteSqlRawAsync(@"
                        IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
                        BEGIN
                            CREATE TABLE [__EFMigrationsHistory] (
                                [MigrationId] nvarchar(150) NOT NULL,
                                [ProductVersion] nvarchar(32) NOT NULL,
                                CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
                            );
                        END;
                        IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260820034608_InitialCreate')
                        BEGIN
                            INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
                            VALUES (N'20260820034608_InitialCreate', N'8.0.14');
                        END;
                    ");
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Không thể ghi bản ghi baseline migration.");
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Lưu ý khi chạy MigrateAsync: {Message}", ex.Message);
            }

            // 2. Seed Roles
            try
            {
                string[] roles = { "Admin", "Staff", "Warehouse", "Customer" };
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                        logger.LogInformation("Created role: {Role}", role);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Lỗi khi khởi tạo roles: {Message}", ex.Message);
            }

            // 3. Seed Default Accounts
            try
            {
                var defaultUsers = new (string UserName, string Email, string FullName, string Role, string Password)[]
                {
                    ("admin", "admin@bookstore.com", "Quản trị viên Hệ thống", "Admin", "Admin@123"),
                    ("staff", "staff@bookstore.com", "Nhân viên CSKH & Vận hành", "Staff", "Staff@123"),
                    ("warehouse", "warehouse@bookstore.com", "Thủ kho MindBook", "Warehouse", "Warehouse@123"),
                    ("customer", "customer@gmail.com", "Nguyễn Văn Đọc Sách", "Customer", "Customer@123")
                };

                foreach (var item in defaultUsers)
                {
                    var user = await userManager.FindByNameAsync(item.UserName);
                    if (user == null)
                    {
                        user = new ApplicationUser
                        {
                            UserName = item.UserName,
                            Email = item.Email,
                            FullName = item.FullName,
                            EmailConfirmed = true,
                            Status = true,
                            WalletBalance = 500000,
                            FPoints = 250,
                            TotalSpend = 1500000,
                            CreatedAt = DateTime.UtcNow
                        };

                        var res = await userManager.CreateAsync(user, item.Password);
                        if (res.Succeeded)
                        {
                            await userManager.AddToRoleAsync(user, item.Role);
                            logger.LogInformation("Seeded default account: {UserName} ({Role})", item.UserName, item.Role);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Lỗi khi khởi tạo tài khoản mặc định: {Message}", ex.Message);
            }

            // 4. Seed Categories
            if (!await context.Categories.AnyAsync())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Văn học & Tiểu thuyết", Description = "Tiểu thuyết kinh điển, văn học hiện đại và tác phẩm đoạt giải" },
                    new Category { Name = "Tâm lý & Kỹ năng sống", Description = "Phát triển bản thân, kỹ năng giao tiếp và tâm lý học ứng dụng" },
                    new Category { Name = "Kinh tế & Đầu tư", Description = "Kinh doanh, khởi nghiệp, tài chính cá nhân và đầu tư chứng khoán" },
                    new Category { Name = "Manga - Comic", Description = "Truyện tranh Nhật Bản, Conan, One Piece, Doraemon và comic Âu Mỹ" },
                    new Category { Name = "Thiếu nhi", Description = "Truyện cổ tích, sách tranh và kiến thức khoa học thiếu nhi" },
                    new Category { Name = "Lịch sử - Xã hội", Description = "Lịch sử Việt Nam, văn hóa thế giới và khảo cứu xã hội" }
                };

                context.Categories.AddRange(categories);
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded Categories");
            }

            // 5. Seed Suppliers
            if (!await context.Suppliers.AnyAsync())
            {
                var suppliers = new List<Supplier>
                {
                    new Supplier { Name = "Nhà xuất bản Trẻ", ContactPerson = "Trần Minh", Phone = "02839316289", Email = "hopdong@nxbtre.com.vn", Address = "161B Lý Chính Thắng, Q3, TP.HCM", IsActive = true },
                    new Supplier { Name = "Nhà xuất bản Kim Đồng", ContactPerson = "Lê Hoàng", Phone = "02439434730", Email = "kinhdoanh@nxbkimdong.com.vn", Address = "55 Quang Trung, Hai Bà Trưng, Hà Nội", IsActive = true },
                    new Supplier { Name = "Công ty CP Văn hóa First News - Trí Việt", ContactPerson = "Nguyễn Hữu Trí", Phone = "02838227979", Email = "triviet@firstnews.com.vn", Address = "11H Nguyễn Thị Minh Khai, Q1, TP.HCM", IsActive = true }
                };

                context.Suppliers.AddRange(suppliers);
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded Suppliers");
            }

            // 6. Seed Locations
            if (!await context.Locations.AnyAsync())
            {
                var locations = new List<Location>
                {
                    new Location { Zone = "A", Rack = "01", Shelf = "01", LocationCode = "A-01-01", Description = "Kệ sách Văn học & Tiểu thuyết tầng 1" },
                    new Location { Zone = "A", Rack = "01", Shelf = "02", LocationCode = "A-01-02", Description = "Kệ sách Văn học & Tiểu thuyết tầng 2" },
                    new Location { Zone = "B", Rack = "01", Shelf = "01", LocationCode = "B-01-01", Description = "Kệ sách Tâm lý & Kỹ năng tầng 1" },
                    new Location { Zone = "M", Rack = "01", Shelf = "01", LocationCode = "M-01-01", Description = "Khu vực Manga - Comic đặc biệt" }
                };

                context.Locations.AddRange(locations);
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded Locations");
            }

            // 7. Seed Books
            if (!await context.Books.AnyAsync())
            {
                var vanHoc = await context.Categories.FirstOrDefaultAsync(c => c.Name.Contains("Văn học"));
                var tamLy = await context.Categories.FirstOrDefaultAsync(c => c.Name.Contains("Tâm lý"));
                var kinhTe = await context.Categories.FirstOrDefaultAsync(c => c.Name.Contains("Kinh tế"));
                var manga = await context.Categories.FirstOrDefaultAsync(c => c.Name.Contains("Manga"));

                var locA1 = await context.Locations.FirstOrDefaultAsync(l => l.LocationCode == "A-01-01");
                var locB1 = await context.Locations.FirstOrDefaultAsync(l => l.LocationCode == "B-01-01");
                var locM1 = await context.Locations.FirstOrDefaultAsync(l => l.LocationCode == "M-01-01");

                context.Books.AddRange(books);
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded Books");
            }

            // 8. Seed Vouchers
            if (!await context.Vouchers.AnyAsync())
            {
                var vouchers = new List<Voucher>
                {
                    new Voucher
                    {
                        Code = "MINDBOOK20",
                        DiscountPercent = 20,
                        MinOrderValue = 150000,
                        MaxDiscount = 50000,
                        StartDate = DateTime.UtcNow.AddDays(-5),
                        EndDate = DateTime.UtcNow.AddDays(60),
                        UsageLimit = 200,
                        Status = 1
                    },
                    new Voucher
                    {
                        Code = "FREESHIP50",
                        DiscountAmount = 30000,
                        MinOrderValue = 100000,
                        StartDate = DateTime.UtcNow.AddDays(-1),
                        EndDate = DateTime.UtcNow.AddDays(30),
                        UsageLimit = 500,
                        Status = 1
                    },
                    new Voucher
                    {
                        Code = "WELCOME50",
                        DiscountAmount = 50000,
                        MinOrderValue = 200000,
                        StartDate = DateTime.UtcNow.AddDays(-10),
                        EndDate = DateTime.UtcNow.AddDays(90),
                        UsageLimit = 100,
                        Status = 1
                    }
                };

                context.Vouchers.AddRange(vouchers);
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded Vouchers");
            }
        }
    }
}

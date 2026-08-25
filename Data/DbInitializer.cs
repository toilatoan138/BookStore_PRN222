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

            // 2. Seed Roles cơ bản
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
            // 3. Seed Chi nhánh (Branches)
            try
            {
                if (!await context.Branches.AnyAsync())
                {
                    context.Branches.AddRange(
                        new Branch { Name = "Chi nhánh Hà Nội", Address = "Đống Đa, Hà Nội", City = "Hà Nội", IsActive = true },
                        new Branch { Name = "Chi nhánh Đà Nẵng", Address = "Sơn Trà, Đà Nẵng", City = "Đà Nẵng", IsActive = true },
                        new Branch { Name = "Chi nhánh TP.HCM", Address = "Quận 1, TP.HCM", City = "TP.HCM", IsActive = true }
                    );
                    await context.SaveChangesAsync();
                    logger.LogInformation("Created 3 initial branches.");
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Lỗi khi khởi tạo chi nhánh: {Message}", ex.Message);
            }

            // 4. Seed Tài khoản (Users)
            try
            {
                var branchHn = await context.Branches.FirstOrDefaultAsync(b => b.Name.Contains("Hà Nội"));
                var branchDn = await context.Branches.FirstOrDefaultAsync(b => b.Name.Contains("Đà Nẵng"));
                var branchHcm = await context.Branches.FirstOrDefaultAsync(b => b.Name.Contains("TP.HCM"));

                int? hnId = branchHn?.Id;
                int? dnId = branchDn?.Id;
                int? hcmId = branchHcm?.Id;

                var usersToSeed = new List<(string UserName, string Email, string FullName, string Role, int? BranchId)>
                {
                    ("superadmin", "superadmin@bookstore.com", "Nguyễn Nhật Anh (Tổng GĐ)", "Admin", null),
                    ("admin_hn", "admin_hn@bookstore.com", "Trưởng Khoa (Admin HN)", "Admin", hnId),
                    ("admin_dn", "admin_dn@bookstore.com", "Lê Hải (Admin ĐN)", "Admin", dnId),
                    ("admin_hcm", "admin_hcm@bookstore.com", "Trần Khắc (Admin HCM)", "Admin", hcmId),
                    
                    ("warehouse_hn", "warehouse_hn@bookstore.com", "Nguyễn Văn Kho (Kho HN)", "Warehouse", hnId),
                    ("warehouse_dn", "warehouse_dn@bookstore.com", "Lê Văn Kho (Kho ĐN)", "Warehouse", dnId),
                    ("warehouse_hcm", "warehouse_hcm@bookstore.com", "Trần Văn Kho (Kho HCM)", "Warehouse", hcmId),
                    
                    ("staff_hn", "staff_hn@bookstore.com", "Phạm Thị Bán (Staff HN)", "Staff", hnId),
                    ("staff_dn", "staff_dn@bookstore.com", "Ngô Thị Bán (Staff ĐN)", "Staff", dnId),
                    ("staff_hcm", "staff_hcm@bookstore.com", "Vũ Thị Bán (Staff HCM)", "Staff", hcmId)
                };

                foreach (var u in usersToSeed)
                {
                    if (await userManager.FindByNameAsync(u.UserName) == null && await userManager.FindByEmailAsync(u.Email) == null)
                    {
                        var newUser = new ApplicationUser
                        {
                            UserName = u.UserName,
                            Email = u.Email,
                            FullName = u.FullName,
                            EmailConfirmed = true,
                            Status = true,
                            FPoints = 0,
                            WalletBalance = 0,
                            BranchId = u.BranchId
                        };

                        var result = await userManager.CreateAsync(newUser, "Password@123");
                        if (result.Succeeded)
                        {
                            await userManager.AddToRoleAsync(newUser, u.Role);
                            logger.LogInformation("Created user {UserName} with role {Role} and BranchId {BranchId}", u.UserName, u.Role, u.BranchId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Lỗi khi khởi tạo tài khoản mồi: {Message}", ex.Message);
            }
        }
    }
}

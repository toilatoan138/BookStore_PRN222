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
        }
    }
}

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
            // 1. Check or apply migrations
            await context.Database.EnsureCreatedAsync();

            // 2. Seed Roles
            string[] roles = { "Admin", "Staff", "Warehouse", "Customer" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                    logger.LogInformation("Created role: {Role}", role);
                }
            }

            // 3. Seed Default Accounts
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

                var books = new List<Book>
                {
                    new Book
                    {
                        Title = "Đắc Nhân Tâm (How to Win Friends and Influence People)",
                        Author = "Dale Carnegie",
                        CategoryId = tamLy?.Id ?? 1,
                        Price = 86000,
                        ImportPrice = 50000,
                        StockQuantity = 45,
                        SoldQuantity = 120,
                        ImageUrl = "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=400&q=80",
                        Publisher = "NXB Tổng Hợp TP.HCM",
                        LocationCode = "B-01-01",
                        IsActive = true,
                        Description = "Đắc Nhân Tâm là cuốn sách nổi tiếng nhất, có tầm ảnh hưởng lớn nhất mọi thời đại về nghệ thuật đối nhân xử thế và thu phục lòng người."
                    },
                    new Book
                    {
                        Title = "Nhà Giả Kim (The Alchemist)",
                        Author = "Paulo Coelho",
                        CategoryId = vanHoc?.Id ?? 1,
                        Price = 79000,
                        ImportPrice = 45000,
                        StockQuantity = 30,
                        SoldQuantity = 95,
                        ImageUrl = "https://images.unsplash.com/photo-1512820790803-83ca734da794?w=400&q=80",
                        Publisher = "NXB Hội Nhà Văn",
                        LocationCode = "A-01-01",
                        IsActive = true,
                        Description = "Hành trình tìm kiếm kho báu của chàng chăn cừu Santiago là câu chuyện truyền cảm hứng sâu sắc về việc theo đuổi ước mơ và lắng nghe trái tim mình."
                    },
                    new Book
                    {
                        Title = "Thám Tử Lừng Danh Conan - Tập 100",
                        Author = "Gosho Aoyama",
                        CategoryId = manga?.Id ?? 1,
                        Price = 30000,
                        ImportPrice = 18000,
                        StockQuantity = 80,
                        SoldQuantity = 250,
                        ImageUrl = "https://images.unsplash.com/photo-1607604276583-eef5d076aa5f?w=400&q=80",
                        Publisher = "NXB Kim Đồng",
                        LocationCode = "M-01-01",
                        IsActive = true,
                        Description = "Tập 100 kỷ niệm cột mốc lịch sử với những vụ án gay cấn và cuộc đối đầu nghẹt thở giữa Conan và Tổ chức Áo đen."
                    },
                    new Book
                    {
                        Title = "One Piece - Tập 105: Giấc mơ của Luffy",
                        Author = "Eiichiro Oda",
                        CategoryId = manga?.Id ?? 1,
                        Price = 35000,
                        ImportPrice = 20000,
                        StockQuantity = 90,
                        SoldQuantity = 310,
                        ImageUrl = "https://images.unsplash.com/photo-1569701813229-33284b643e3c?w=400&q=80",
                        Publisher = "NXB Kim Đồng",
                        LocationCode = "M-01-01",
                        IsActive = true,
                        Description = "Sau chiến thắng lịch sử tại Vương quốc Wano, Luffy chính thức trở thành Tứ Hoàng và bắt đầu hành trình đến hòn đảo tương lai Egghead."
                    },
                    new Book
                    {
                        Title = "Cha Giàu Cha Nghèo (Rich Dad Poor Dad)",
                        Author = "Robert T. Kiyosaki",
                        CategoryId = kinhTe?.Id ?? 1,
                        Price = 115000,
                        ImportPrice = 65000,
                        StockQuantity = 25,
                        SoldQuantity = 78,
                        ImageUrl = "https://images.unsplash.com/photo-1592496431122-2349e0fbc666?w=400&q=80",
                        Publisher = "NXB Trẻ",
                        LocationCode = "B-01-01",
                        IsActive = true,
                        Description = "Cuốn sách số 1 thế giới về tài chính cá nhân, giúp bạn hiểu rõ sự khác biệt giữa tài sản và tiêu sản để đạt tự do tài chính."
                    },
                    new Book
                    {
                        Title = "Tuổi Trẻ Đáng Giá Bao Nhiêu?",
                        Author = "Rosie Nguyễn",
                        CategoryId = tamLy?.Id ?? 1,
                        Price = 75000,
                        ImportPrice = 42000,
                        StockQuantity = 4, // Low stock test
                        SoldQuantity = 140,
                        ImageUrl = "https://images.unsplash.com/photo-1497633762265-9d179a990aa6?w=400&q=80",
                        Publisher = "NXB Nhã Nam",
                        LocationCode = "B-01-01",
                        IsActive = true,
                        Description = "Cuốn sách kim chỉ nam dành cho những người trẻ đang băn khoăn tìm kiếm định hướng và giá trị của bản thân."
                    }
                };

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

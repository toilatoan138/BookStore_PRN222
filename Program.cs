using BookStore.Data;
using BookStore.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ====================================================================
// 1. DATABASE & EF CORE
// ====================================================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ====================================================================
// 2. ASP.NET CORE IDENTITY
// ====================================================================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password rules
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // User rules
    options.User.RequireUniqueEmail = true;

    // Lockout
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ====================================================================
// 3. COOKIE / LOGIN PATH CONFIGURATION
// ====================================================================
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

// ====================================================================
// 4. SESSION & SERVICES DI
// ====================================================================
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<BookStore.Services.IEmailService, BookStore.Services.EmailService>();
builder.Services.AddScoped<BookStore.Services.IUserService, BookStore.Services.UserService>();
builder.Services.AddScoped<BookStore.Services.IBookService, BookStore.Services.BookService>();
builder.Services.AddScoped<BookStore.Services.ICartService, BookStore.Services.CartService>();
builder.Services.AddScoped<BookStore.Services.IVnPayService, BookStore.Services.VnPayService>();
builder.Services.AddScoped<BookStore.Services.IOrderService, BookStore.Services.OrderService>();
builder.Services.AddScoped<BookStore.Services.ICollectionService, BookStore.Services.CollectionService>();
builder.Services.AddScoped<BookStore.Services.IVoucherService, BookStore.Services.VoucherService>();
builder.Services.AddScoped<BookStore.Services.INotificationService, BookStore.Services.NotificationService>();
builder.Services.AddScoped<BookStore.Services.ISupportTicketService, BookStore.Services.SupportTicketService>();
builder.Services.AddScoped<BookStore.Services.IAdminService, BookStore.Services.AdminService>();
builder.Services.AddScoped<BookStore.Services.ICategoryService, BookStore.Services.CategoryService>();
builder.Services.AddScoped<BookStore.Services.IStaffService, BookStore.Services.StaffService>();
builder.Services.AddScoped<BookStore.Services.IWarehouseService, BookStore.Services.WarehouseService>();
builder.Services.AddScoped<BookStore.Services.IWarehouseFulfillmentService, BookStore.Services.WarehouseFulfillmentService>();
builder.Services.AddSignalR();

// ====================================================================
// 5. RAZOR PAGES + AUTHORIZATION BY FOLDER
// ====================================================================
builder.Services.AddRazorPages(options =>
{
    // Public pages (anonymous access)
    options.Conventions.AllowAnonymousToPage("/Index");
    options.Conventions.AllowAnonymousToPage("/Search");
    options.Conventions.AllowAnonymousToPage("/Products/Detail");
    options.Conventions.AllowAnonymousToPage("/Products/FlashSale");
    options.Conventions.AllowAnonymousToPage("/Products/Conan");
    options.Conventions.AllowAnonymousToPage("/Products/OnePiece");
    options.Conventions.AllowAnonymousToPage("/Account/Login");
    options.Conventions.AllowAnonymousToPage("/Account/Register");
    options.Conventions.AllowAnonymousToPage("/Account/ForgotPassword");
    options.Conventions.AllowAnonymousToPage("/Account/PublicProfile");
    options.Conventions.AllowAnonymousToPage("/Error");

    // Protected folders — require specific roles
    options.Conventions.AuthorizeFolder("/Admin", "RequireAdminRole");
    options.Conventions.AuthorizeFolder("/Staff", "RequireStaffRole");
    options.Conventions.AuthorizeFolder("/Warehouse", "RequireWarehouseRole");

    // Protected folders — require any authenticated user
    options.Conventions.AuthorizeFolder("/Cart");
    options.Conventions.AuthorizeFolder("/Checkout");
    options.Conventions.AuthorizeFolder("/Orders");
    options.Conventions.AuthorizeFolder("/Collections");
    options.Conventions.AuthorizeFolder("/Vouchers");
    options.Conventions.AuthorizeFolder("/Support");
    options.Conventions.AuthorizeFolder("/Notifications");
});

// ====================================================================
// 6. AUTHORIZATION POLICIES
// ====================================================================
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireStaffRole", policy => policy.RequireRole("Staff", "Admin"));
    options.AddPolicy("RequireWarehouseRole", policy => policy.RequireRole("Warehouse", "Admin"));
});

// ====================================================================
// 7. LOGGING
// ====================================================================
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// ====================================================================
// BUILD APP
// ====================================================================
var app = builder.Build();

// ====================================================================
// MIDDLEWARE PIPELINE
// ====================================================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapHub<BookStore.Hubs.ChatHub>("/chatHub");

// ====================================================================
// SEED DATABASE ON STARTUP (Roles, Accounts, Categories, Books, etc.)
// ====================================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var context = services.GetRequiredService<BookStore.Data.ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<BookStore.Models.Entities.ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await BookStore.Data.DbInitializer.InitializeAsync(context, userManager, roleManager, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// --- CŨ ĐÃ XÓA VÀ CHUYỂN SANG DbInitializer ---

app.Run();

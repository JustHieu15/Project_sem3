using Microsoft.EntityFrameworkCore;
using AspnetCoreMvcStarter.Context;
using AspnetCoreMvcStarter.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies; // Thêm cho Cookie Authentication

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<AuthAttribute>(); // Giữ nguyên bộ lọc toàn cục
});

// Cấu hình DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cấu hình Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Thêm dịch vụ xác thực với Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login"; // Đường dẫn đến trang đăng nhập (sử dụng AuthController)
        options.AccessDeniedPath = "/Auth/AccessDenied"; // Đường dẫn khi bị từ chối quyền
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Thời gian hết hạn của cookie
    });

// Thêm dịch vụ phân quyền
builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Seed admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        // context.Database.Migrate(); // Nếu dùng migrations, thay EnsureCreated bằng Migrate
        SeedAdminUser(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Thêm middleware xác thực trước phân quyền và session
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Hàm seed admin user
void SeedAdminUser(ApplicationDbContext context)
{
    if (!context.Users.Any(u => u.Role == UserRole.Admin))
    {
        var adminUser = new User
        {
            Name = "Admin User",
            Email = "admin@example.com",
            Password = HashPasswordStatic("Admin@123"),
            Role = UserRole.Admin,
            Address = "Admin Address",
            Phone = "0123456789",
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        context.Users.Add(adminUser);
        context.SaveChanges();
        Console.WriteLine("Admin user seeded successfully.");
    }
    else
    {
        Console.WriteLine("Admin user already exists.");
    }
}

// Hàm hash tĩnh để sử dụng trong Program.cs
string HashPasswordStatic(string password)
{
    using (SHA256 sha256Hash = SHA256.Create())
    {
        byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < bytes.Length; i++)
        {
            builder.Append(bytes[i].ToString("x2"));
        }
        return builder.ToString();
    }
}

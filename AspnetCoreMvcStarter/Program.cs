using Microsoft.EntityFrameworkCore;
using AspnetCoreMvcStarter.Context;
using AspnetCoreMvcStarter.Models; // Cần cho User và UserRole
using System.Security.Cryptography; // Cần cho SHA256
using System.Text; // Cần cho Encoding

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<AuthAttribute>(); // Đảm bảo AuthAttribute được đăng ký toàn cục
});

// Cấu hình DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cấu hình Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian session hết hạn
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();


var app = builder.Build();

// Seed admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        // Đảm bảo database đã được tạo (nếu chưa có)
        // context.Database.EnsureCreated(); // Hoặc chạy migrations
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

app.UseSession(); // Quan trọng: Phải đứng trước UseAuthorization và UseEndpoints

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


// Hàm seed admin user
void SeedAdminUser(ApplicationDbContext context)
{
    // Kiểm tra xem đã có admin user nào chưa
    if (!context.Users.Any(u => u.Role == UserRole.Admin))
    {
        var adminUser = new User
        {
            Name = "Admin User",
            Email = "admin@example.com", // Thay bằng email admin bạn muốn
            Password = HashPasswordStatic("Admin@123"), // Thay bằng mật khẩu admin bạn muốn
            Role = UserRole.Admin,
            Address = "Admin Address",
            Phone = "0123456789",
            IsActive = true,
            CreatedDate = DateTime.UtcNow
            // DepartmentId có thể để null hoặc gán nếu có Department mặc định
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

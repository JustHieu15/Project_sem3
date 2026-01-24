using Microsoft.EntityFrameworkCore;
using AspnetCoreMvcStarter.Context;
using AspnetCoreMvcStarter.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<AuthAttribute>();
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
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// Thêm dịch vụ xác thực với Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.LogoutPath = "/Auth/Logout";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

// Thêm dịch vụ phân quyền
builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Seed admin user and default department
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        SeedData(context);
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

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Hàm seed admin user and department
void SeedData(ApplicationDbContext context)
{
    // Seed default department
    if (!context.Departments.Any())
    {
        context.Departments.Add(new Department
        {
            Name = "Phòng Ban Mặc Định",
            Code = "DEPT001",
            CreatedDate = DateTime.UtcNow,
            Description = "Phòng ban mặc định cho hệ thống"
        });
        context.SaveChanges();
        Console.WriteLine("Default department seeded successfully.");
    }
    else
    {
        Console.WriteLine("Departments already exist.");
    }

    // Seed admin user
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
            CreatedDate = DateTime.UtcNow,
            DepartmentId = context.Departments.FirstOrDefault()?.Id
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

// Hàm hash tĩnh
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

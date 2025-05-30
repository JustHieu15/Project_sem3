using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography; // Cần cho SHA256
using System.Text; // Cần cho Encoding
using AspnetCoreMvcStarter.Context;
using AspnetCoreMvcStarter.Models; // Cần cho User và UserRole

namespace AspnetCoreMvcStarter.Controllers
{
  public class AuthController : Controller
  {
    private readonly ApplicationDbContext _context;

    public AuthController(ApplicationDbContext context)
    {
      _context = context;
    }

    public IActionResult Login()
    {
      // Nếu người dùng đã đăng nhập, chuyển hướng họ đến trang chủ
      if (HttpContext.Session.GetInt32("UserId") != null)
      {
          return RedirectToAction("Index", "Home");
      }
      return View();
    }

    [HttpPost]
    public IActionResult Login(string email, string password)
    {
      if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
      {
          ViewBag.Error = "Vui lòng nhập email và mật khẩu.";
          return View();
      }

      var hashedPassword = HashPassword(password); // Hash mật khẩu người dùng nhập
      var user = _context.Users.FirstOrDefault(u => u.Email == email && u.Password == hashedPassword);

      if (user != null)
      {
        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("UserName", user.Name ?? "User"); // Đảm bảo Name không null
        HttpContext.Session.SetString("UserRole", user.Role.ToString()); // Lưu vai trò vào session

        // Chuyển hướng dựa trên vai trò nếu cần, ví dụ:
        // if (user.Role == UserRole.Admin) {
        //     return RedirectToAction("Dashboard", "Admin");
        // }
        return RedirectToAction("Index", "Home");
      }

      ViewBag.Error = "Email hoặc mật khẩu không đúng!";
      return View();
    }

    public IActionResult Logout()
    {
      HttpContext.Session.Clear();
      return RedirectToAction("Login");
    }

    // Hàm hash mật khẩu (giống trong UserController)
    private string HashPassword(string password)
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
  }
}

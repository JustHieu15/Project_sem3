using AspnetCoreMvcStarter.Areas.Admin.Context;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

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
      return View();
    }

    [HttpPost]
    public IActionResult Login(string email, string password)
    {
      var user = _context.Users.FirstOrDefault(u => u.Email == email && u.Password == password);

      if (user != null)
      {
        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("UserName", user.Name);
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


  }
}

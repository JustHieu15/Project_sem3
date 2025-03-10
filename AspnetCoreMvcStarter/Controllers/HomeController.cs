using Microsoft.AspNetCore.Mvc;

namespace AspnetCoreMvcStarter.Controllers;

public class HomeController: AdminBaseController
{
  public IActionResult Index()
  {
    return View();
  }
}

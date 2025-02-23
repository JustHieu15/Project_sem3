using Microsoft.AspNetCore.Mvc;

namespace AspnetCoreMvcStarter.Areas.Admin.Controllers;

public class HomeController: AdminBaseController
{
  public IActionResult Index()
  {
    return View();
  }
}

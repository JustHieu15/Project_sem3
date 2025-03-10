using Microsoft.AspNetCore.Mvc;

namespace AspnetCoreMvcStarter.Controllers;

public class DashboardController: AdminBaseController
{
  public IActionResult Index()
  {
    return View();
  }
}

using Microsoft.AspNetCore.Mvc;

namespace AspnetCoreMvcStarter.Areas.Admin.Controllers;

public class DashboardController: AdminBaseController
{
  public IActionResult Index()
  {
    return View();
  }
}

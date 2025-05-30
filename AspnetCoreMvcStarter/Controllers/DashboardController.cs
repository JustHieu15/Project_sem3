using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcStarter.Context;

namespace AspnetCoreMvcStarter.Controllers;

public class DashboardController: AdminBaseController
{
  public IActionResult Index()
  {
    return View();
  }
}

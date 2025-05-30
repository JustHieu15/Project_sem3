using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcStarter.Context;

namespace AspnetCoreMvcStarter.Controllers;

public class HomeController: AdminBaseController
{
  public IActionResult Index()
  {
    return View();
  }
}

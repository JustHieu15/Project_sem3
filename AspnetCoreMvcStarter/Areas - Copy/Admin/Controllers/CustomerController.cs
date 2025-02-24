using Microsoft.AspNetCore.Mvc;

namespace AspnetCoreMvcStarter.Areas.Admin.Controllers;

public class CustomersController : AdminBaseController
{
  public IActionResult CustomerAll()
  {
    return View();
  }
  public IActionResult CustomerDetailsBilling()
  {
    return View();
  }
  public IActionResult CustomerDetailsNotifications()
  {
    return View();
  }
  public IActionResult CustomerDetailsOverview()
  {
    return View();
  }
  public IActionResult CustomerDetailsSecurity()
  {
    return View();
  }
}

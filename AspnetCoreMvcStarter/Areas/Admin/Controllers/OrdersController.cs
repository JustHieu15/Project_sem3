using Microsoft.AspNetCore.Mvc;

namespace AspnetCoreMvcStarter.Areas.Admin.Controllers;

public class OrdersController : AdminBaseController
{
  public IActionResult OrderList()
  {
    return View();
  }
  public IActionResult OrderDetails()
  {
    return View();
  }
}

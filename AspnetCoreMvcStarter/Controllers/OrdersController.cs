using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcStarter.Context;

namespace AspnetCoreMvcStarter.Controllers;

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

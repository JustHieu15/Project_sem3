using Microsoft.AspNetCore.Mvc;

namespace AspnetCoreMvcStarter.Areas.Admin.Controllers;

public class ServicesController : AdminBaseController
{
  public IActionResult ServiceAdd()
  {
    return View();
  }public IActionResult ServiceCategoryList()
  {
    return View();
  }
  public IActionResult ServiceList()
  {
    return View();
  }
}

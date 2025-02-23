using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

using AspnetCoreMvcStarter.Areas.Admin.Controllers;

namespace AspnetCoreMvcStarter.Areas.Admin.Controllers;

public class ChartsController : AdminBaseController
{
  public IActionResult Apex() => View();
  public IActionResult Chartjs() => View();
}

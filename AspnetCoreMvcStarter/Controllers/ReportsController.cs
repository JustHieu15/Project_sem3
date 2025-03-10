using AspnetCoreMvcStarter.Areas.Admin.Context;
using AspnetCoreMvcStarter.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspnetCoreMvcStarter.Controllers
{
  public class ReportsController : Controller
  {
    private readonly ApplicationDbContext _context;

    public ReportsController(ApplicationDbContext context)
    {
      _context = context;
    }

    public async Task<IActionResult> ServiceRevenue()
    {
      var reportData = await _context.Services
      .Select(s => new ServiceRevenueViewModel
      {
        ServiceName = s.Name,
        ServicePrice = s.Price,
        UsageCount = s.Invoices.Count(i => i.StatusPayment == 1),
        TotalRevenue = s.Invoices.Where(i => i.StatusPayment == 1).Sum(i => (decimal?)i.TotalAmount) ?? 0
      })
      .ToListAsync(); // Chuyển thành danh sách trước

      reportData = reportData.OrderByDescending(r => r.TotalRevenue).ToList(); // Sắp xếp sau


      return View(reportData);
    }



  }
}

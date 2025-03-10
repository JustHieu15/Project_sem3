using AspnetCoreMvcStarter.Areas.Admin.Context;
using AspnetCoreMvcStarter.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspnetCoreMvcStarter.Controllers;

public class ServicesController : AdminBaseController
{
    private readonly ApplicationDbContext _context;

    public ServicesController(ApplicationDbContext context)
    {
      _context = context;
    }

    // GET: Services (Hiển thị danh sách dịch vụ)
    public async Task<IActionResult> Index()
    {
      return View(await _context.Services.ToListAsync());
    }

    // GET: Services/Details/5 (Xem chi tiết)
    public async Task<IActionResult> Details(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var service = await _context.Services.FirstOrDefaultAsync(m => m.Id == id);
      if (service == null)
      {
        return NotFound();
      }

      return View(service);
    }

    // GET: Services/Create (Form thêm mới)
    public IActionResult Create()
    {
      return View();
    }

    // POST: Services/Create (Lưu dịch vụ mới)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name,Description,Price,Status")] Services service)
    {
        _context.Add(service);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: Services/Edit/5 (Form cập nhật)
    public async Task<IActionResult> Edit(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var service = await _context.Services.FindAsync(id);
      if (service == null)
      {
        return NotFound();
      }
      return View(service);
    }

    // POST: Services/Edit/5 (Cập nhật dịch vụ)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,Status")] Services service)
    {
      if (id != service.Id)
      {
        return NotFound();
      }

        try
        {
          _context.Update(service);
          await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
          if (!ServiceExists(service.Id))
          {
            return NotFound();
          }
          else
          {
            throw;
          }
        }
        return RedirectToAction(nameof(Index));
    }

    // GET: Services/Delete/5 (Xác nhận xóa)
    public async Task<IActionResult> Delete(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var service = await _context.Services.FirstOrDefaultAsync(m => m.Id == id);
      if (service == null)
      {
        return NotFound();
      }

      return View(service);
    }

    // POST: Services/Delete/5 (Xóa dịch vụ)
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
      var service = await _context.Services.FindAsync(id);
      if (service != null)
      {
        _context.Services.Remove(service);
        await _context.SaveChangesAsync();
      }
      return RedirectToAction(nameof(Index));
    }

    private bool ServiceExists(int id)
    {
      return _context.Services.Any(e => e.Id == id);
    }

  [HttpGet]
  public IActionResult GetServicePrice(int serviceId)
  {
    var service = _context.Services.FirstOrDefault(s => s.Id == serviceId);
    if (service == null)
    {
      return NotFound();
    }
    return Json(new { price = service.Price });
  }





}

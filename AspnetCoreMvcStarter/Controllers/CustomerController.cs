using AspnetCoreMvcStarter.Areas.Admin.Context;
using AspnetCoreMvcStarter.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspnetCoreMvcStarter.Controllers
{
  public class CustomerController : Controller
  {
    private readonly ApplicationDbContext _context;

    public CustomerController(ApplicationDbContext context)
    {
      _context = context;
    }

    // GET: Customers
    public async Task<IActionResult> Index()
    {
      return View(await _context.Users.Where(u => u.Role == "customer").ToListAsync());
    }

    // GET: Customers/Details/5
    public async Task<IActionResult> Details(int? id)
    {
      if (id == null)
        return NotFound();

      var customer = await _context.Users.FirstOrDefaultAsync(m => m.Id == id && m.Role == "customer");
      if (customer == null)
        return NotFound();

      return View(customer);
    }

    // GET: Customers/Create
    public IActionResult Create()
    {
      return View();
    }

    // POST: Customers/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(User customer)
    {
 
        customer.Role = "customer";
        customer.Password = "";
        _context.Add(customer);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: Customers/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
      if (id == null)
        return NotFound();

      var customer = await _context.Users.FindAsync(id);
      if (customer == null || customer.Role != "customer")
        return NotFound();

      return View(customer);
    }

    // POST: Customers/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, User customer)
    {
      if (id != customer.Id)
        return NotFound();

     
        try
        {
          customer.Role = "customer";
        customer.Password = "";
          _context.Update(customer);
          await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
          if (!_context.Users.Any(e => e.Id == customer.Id))
            return NotFound();
          else
            throw;
        }
        return RedirectToAction(nameof(Index));

    }

    // GET: Customers/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
      if (id == null)
        return NotFound();

      var customer = await _context.Users.FirstOrDefaultAsync(m => m.Id == id && m.Role == "customer");
      if (customer == null)
        return NotFound();

      return View(customer);
    }

    // POST: Customers/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
      var customer = await _context.Users.FindAsync(id);
      if (customer != null && customer.Role == "customer")
      {
        _context.Users.Remove(customer);
        await _context.SaveChangesAsync();
      }
      return RedirectToAction(nameof(Index));
    }
























  }
}

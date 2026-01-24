using AspnetCoreMvcStarter.Context;
using AspnetCoreMvcStarter.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspnetCoreMvcStarter.Controllers
{
    [Auth] // Sử dụng custom Auth attribute thay vì Authorize
    public class DepartmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DepartmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Departments
        public async Task<IActionResult> Index()
        {
            // Kiểm tra quyền Admin bằng Session
            var userRoleString = HttpContext.Session.GetString("UserRole");
            if (userRoleString != UserRole.Admin.ToString())
            {
                TempData["Error"] = "Bạn không có quyền truy cập chức năng này.";
                return RedirectToAction("Index", "Home");
            }

            return View(await _context.Departments.ToListAsync());
        }

        // GET: Departments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var userRoleString = HttpContext.Session.GetString("UserRole");
            if (userRoleString != UserRole.Admin.ToString())
            {
                TempData["Error"] = "Bạn không có quyền truy cập chức năng này.";
                return RedirectToAction("Index", "Home");
            }

            if (id == null) return NotFound();

            var department = await _context.Departments
                .Include(d => d.Users) // Lấy danh sách user thuộc phòng ban
                .FirstOrDefaultAsync(m => m.Id == id);

            if (department == null) return NotFound();

            return View(department);
        }

        // GET: Departments/Create
        public IActionResult Create()
        {
            var userRoleString = HttpContext.Session.GetString("UserRole");
            if (userRoleString != UserRole.Admin.ToString())
            {
                TempData["Error"] = "Bạn không có quyền truy cập chức năng này.";
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // POST: Departments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Code,Description")] Department department)
        {
            var userRoleString = HttpContext.Session.GetString("UserRole");
            if (userRoleString != UserRole.Admin.ToString())
            {
                TempData["Error"] = "Bạn không có quyền truy cập chức năng này.";
                return RedirectToAction("Index", "Home");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    department.CreatedDate = DateTime.Now;
                    _context.Add(department);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Tạo phòng ban thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Lỗi khi tạo phòng ban: " + ex.Message;
                }
            }
            return View(department);
        }

        // GET: Departments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var userRoleString = HttpContext.Session.GetString("UserRole");
            if (userRoleString != UserRole.Admin.ToString())
            {
                TempData["Error"] = "Bạn không có quyền truy cập chức năng này.";
                return RedirectToAction("Index", "Home");
            }

            if (id == null) return NotFound();

            var department = await _context.Departments.FindAsync(id);
            if (department == null) return NotFound();

            return View(department);
        }

        // POST: Departments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Code,Description,CreatedDate")] Department department)
        {
            var userRoleString = HttpContext.Session.GetString("UserRole");
            if (userRoleString != UserRole.Admin.ToString())
            {
                TempData["Error"] = "Bạn không có quyền truy cập chức năng này.";
                return RedirectToAction("Index", "Home");
            }

            if (id != department.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(department);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật phòng ban thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DepartmentExists(department.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(department);
        }

        // GET: Departments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var userRoleString = HttpContext.Session.GetString("UserRole");
            if (userRoleString != UserRole.Admin.ToString())
            {
                TempData["Error"] = "Bạn không có quyền truy cập chức năng này.";
                return RedirectToAction("Index", "Home");
            }

            if (id == null) return NotFound();

            var department = await _context.Departments.FirstOrDefaultAsync(m => m.Id == id);
            if (department == null) return NotFound();

            return View(department);
        }

        // POST: Departments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userRoleString = HttpContext.Session.GetString("UserRole");
            if (userRoleString != UserRole.Admin.ToString())
            {
                TempData["Error"] = "Bạn không có quyền truy cập chức năng này.";
                return RedirectToAction("Index", "Home");
            }

            var department = await _context.Departments.FindAsync(id);
            if (department != null)
            {
                try
                {
                    _context.Departments.Remove(department);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Xóa phòng ban thành công!";
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Không thể xóa phòng ban: " + ex.Message;
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private bool DepartmentExists(int id)
        {
            return _context.Departments.Any(e => e.Id == id);
        }
    }
}

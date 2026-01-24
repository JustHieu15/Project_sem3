// Controllers/JobTypesController.cs
using AspnetCoreMvcStarter.Context;
using AspnetCoreMvcStarter.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AspnetCoreMvcStarter.Controllers
{
    [Auth] // Sử dụng Auth attribute
    public class JobTypesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public JobTypesController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserRole") == UserRole.Admin.ToString();
        }

        private IActionResult ForbidAdmin()
        {
             if (!IsAdmin())
            {
                TempData["Error"] = "Bạn không có quyền truy cập chức năng này.";
                return RedirectToAction("Index", "Home");
            }
            return null;
        }

        // GET: JobTypes
        public async Task<IActionResult> Index()
        {
            var forbidResult = ForbidAdmin();
            if (forbidResult != null) return forbidResult;

            return View(await _context.JobTypes.ToListAsync());
        }

        // GET: JobTypes/Create
        public IActionResult Create()
        {
            var forbidResult = ForbidAdmin();
            if (forbidResult != null) return forbidResult;

            return View();
        }

        // POST: JobTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Code,Description,Color,DefaultKpiPoints,DefaultRequiredDays,Priority,IsActive,DisplayOrder")] JobType jobType)
        {
            var forbidResult = ForbidAdmin();
            if (forbidResult != null) return forbidResult;

            if (ModelState.IsValid)
            {
                _context.Add(jobType);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Tạo mới loại công việc thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(jobType);
        }

        // GET: JobTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var forbidResult = ForbidAdmin();
            if (forbidResult != null) return forbidResult;

            if (id == null) return NotFound();
            var jobType = await _context.JobTypes.FindAsync(id);
            if (jobType == null) return NotFound();
            return View(jobType);
        }

        // POST: JobTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Code,Description,Color,DefaultKpiPoints,DefaultRequiredDays,Priority,IsActive,DisplayOrder,CreatedDate")] JobType jobType)
        {
            var forbidResult = ForbidAdmin();
            if (forbidResult != null) return forbidResult;

            if (id != jobType.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(jobType);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật loại công việc thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.JobTypes.Any(e => e.Id == jobType.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(jobType);
        }

        // GET: JobTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var forbidResult = ForbidAdmin();
            if (forbidResult != null) return forbidResult;

            if (id == null) return NotFound();
            var jobType = await _context.JobTypes.FirstOrDefaultAsync(m => m.Id == id);
            if (jobType == null) return NotFound();

            return View(jobType);
        }

        // POST: JobTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
          var forbidResult = ForbidAdmin();
          if (forbidResult != null) return forbidResult;

          var jobType = await _context.JobTypes.FindAsync(id);

          // ĐÃ SỬA: Thêm kiểm tra null trước khi xóa
          if (jobType != null)
          {
            _context.JobTypes.Remove(jobType);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Xóa loại công việc thành công!";
          }
          else
          {
            TempData["Error"] = "Không tìm thấy loại công việc để xóa.";
          }

          return RedirectToAction(nameof(Index));
        }
    }
}

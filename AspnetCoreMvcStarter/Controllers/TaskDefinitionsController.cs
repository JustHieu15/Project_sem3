// Controllers/TaskDefinitionsController.cs
using AspnetCoreMvcStarter.Context;
using AspnetCoreMvcStarter.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace AspnetCoreMvcStarter.Controllers
{
    [Auth]
    public class TaskDefinitionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TaskDefinitionsController> _logger; // Thêm logger

        public TaskDefinitionsController(ApplicationDbContext context, ILogger<TaskDefinitionsController> logger)
        {
            _context = context;
            _logger = logger; // Khởi tạo logger
        }

        private bool IsAdmin() => HttpContext.Session.GetString("UserRole") == UserRole.Admin.ToString();
        private IActionResult ForbidAdmin()
        {
            if (!IsAdmin())
            {
                TempData["Error"] = "Bạn không có quyền truy cập chức năng này.";
                return RedirectToAction("Index", "Home");
            }
            return null;
        }

        // Phương thức hỗ trợ để tải Dropdown
        private void PopulateJobTypesDropdown(object selectedJobType = null)
        {
            var jobTypesQuery = _context.JobTypes.Where(jt => jt.IsActive).OrderBy(jt => jt.Name);
            ViewData["JobTypeId"] = new SelectList(jobTypesQuery, "Id", "Name", selectedJobType);
        }

        public async Task<IActionResult> Index()
        {
            var forbidResult = ForbidAdmin(); if (forbidResult != null) return forbidResult;
            var definitions = await _context.TaskDefinitions.Include(t => t.JobType).ToListAsync();
            return View(definitions);
        }

        // GET: TaskDefinitions/Create
        public IActionResult Create()
        {
            var forbidResult = ForbidAdmin(); if (forbidResult != null) return forbidResult;
            PopulateJobTypesDropdown();
            // Trả về một model mới với giá trị mặc định cho checkbox
            return View(new TaskDefinition { IsActive = true });
        }

        // POST: TaskDefinitions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaskDefinition taskDefinition)
        {
            var forbidResult = ForbidAdmin(); if (forbidResult != null) return forbidResult;

            // Xóa validation cho navigation property để tránh lỗi không cần thiết
            ModelState.Remove("JobType");

            if (ModelState.IsValid)
            {
                _context.Add(taskDefinition);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Tạo danh mục công việc thành công!";
                return RedirectToAction(nameof(Index));
            }

            // Nếu ModelState không hợp lệ, ghi log lỗi để debug
            _logger.LogWarning("Create TaskDefinition failed due to invalid ModelState.");
            foreach (var modelStateKey in ModelState.Keys)
            {
                var modelStateVal = ModelState[modelStateKey];
                foreach (var error in modelStateVal.Errors)
                {
                    _logger.LogWarning($"Key: {modelStateKey}, Error: {error.ErrorMessage}");
                }
            }

            // Tải lại dropdown và trả về view với dữ liệu đã nhập
            PopulateJobTypesDropdown(taskDefinition.JobTypeId);
            TempData["Error"] = "Tạo mới thất bại. Vui lòng kiểm tra lại các thông tin đã nhập.";
            return View(taskDefinition);
        }

        // Edit và Delete giữ nguyên...
        public async Task<IActionResult> Edit(int? id)
        {
            var forbidResult = ForbidAdmin(); if (forbidResult != null) return forbidResult;
            if (id == null) return NotFound();
            var taskDefinition = await _context.TaskDefinitions.FindAsync(id);
            if (taskDefinition == null) return NotFound();
            PopulateJobTypesDropdown(taskDefinition.JobTypeId);
            return View(taskDefinition);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaskDefinition taskDefinition)
        {
            var forbidResult = ForbidAdmin(); if (forbidResult != null) return forbidResult;
            if (id != taskDefinition.Id) return NotFound();

            ModelState.Remove("JobType");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(taskDefinition);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật thành công!";
                }
                catch (DbUpdateConcurrencyException) {
                    if (!_context.TaskDefinitions.Any(e => e.Id == id)) return NotFound(); else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            PopulateJobTypesDropdown(taskDefinition.JobTypeId);
            return View(taskDefinition);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            var forbidResult = ForbidAdmin(); if (forbidResult != null) return forbidResult;
            if (id == null) return NotFound();
            var taskDefinition = await _context.TaskDefinitions.Include(t => t.JobType).FirstOrDefaultAsync(m => m.Id == id);
            if (taskDefinition == null) return NotFound();
            return View(taskDefinition);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var forbidResult = ForbidAdmin(); if (forbidResult != null) return forbidResult;
            var taskDefinition = await _context.TaskDefinitions.FindAsync(id);
            if (taskDefinition != null)
            {
                _context.TaskDefinitions.Remove(taskDefinition);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

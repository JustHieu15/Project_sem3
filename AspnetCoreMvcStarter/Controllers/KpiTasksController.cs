// Controllers/KpiTasksController.cs
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
    public class KpiTasksController : Controller
    {
        private readonly ApplicationDbContext _context;
        public KpiTasksController(ApplicationDbContext context) { _context = context; }

        private void PopulateDropdowns(KpiTask task = null)
        {
            ViewData["JobTypeId"] = new SelectList(_context.JobTypes.Where(jt => jt.IsActive), "Id", "Name");
            ViewData["ApproverId"] = new SelectList(_context.Users.Where(u => u.Role == UserRole.Leader), "Id", "Name", task?.ApproverId);
            ViewData["MainDepartmentId"] = new SelectList(_context.Departments, "Id", "Name", task?.MainDepartmentId);
            ViewBag.AllDepartments = _context.Departments.ToList();
        }

        public IActionResult Create()
        {
            PopulateDropdowns();
            return View(new KpiTask { DeadlineInDocument = System.DateTime.Today });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KpiTask kpiTask, string[] RelatedDepartmentIds)
        {
            ModelState.Remove("TaskDefinition");
            ModelState.Remove("AssignedUser");
            ModelState.Remove("Approver");
            ModelState.Remove("MainDepartment");

            if (ModelState.IsValid)
            {
                kpiTask.AssignedUserId = HttpContext.Session.GetInt32("UserId");
                kpiTask.CreatedDate = System.DateTime.Now;
                if (RelatedDepartmentIds != null)
                {
                    kpiTask.RelatedDepartmentIds = string.Join(",", RelatedDepartmentIds);
                }
                _context.Add(kpiTask);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Tạo công việc thành công!";
                return RedirectToAction(nameof(MyTask));
            }
            PopulateDropdowns(kpiTask);
            return View(kpiTask);
        }

        [HttpGet]
        public async Task<JsonResult> GetTaskDefinitionsByJobType(int jobTypeId)
        {
            var taskDefinitions = await _context.TaskDefinitions
                .Where(t => t.JobTypeId == jobTypeId && t.IsActive)
                .Select(t => new { id = t.Id, name = t.Name })
                .ToListAsync();
            return Json(taskDefinitions);
        }

        public async Task<IActionResult> MyTask()
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            var myTasks = await _context.KpiTasks
                .Where(t => t.AssignedUserId == currentUserId)
                .Include(t => t.TaskDefinition)
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync();
            return View(myTasks);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var kpiTask = await _context.KpiTasks
                .Include(k => k.TaskDefinition)
                .Include(k => k.AssignedUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (kpiTask == null) return NotFound();

            var currentUserId = HttpContext.Session.GetInt32("UserId");
            var currentUserRole = HttpContext.Session.GetString("UserRole");
            if (currentUserRole == UserRole.Employee.ToString() && kpiTask.AssignedUserId != currentUserId)
                return Forbid();

            PopulateDropdowns(kpiTask);
            return View(kpiTask);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(int id, KpiTask kpiTask, string[] RelatedDepartmentIds)
        {
            if (id != kpiTask.Id) return NotFound();

            var taskFromDb = await _context.KpiTasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
            if (taskFromDb == null) return NotFound();

            ModelState.Remove("TaskDefinition");
            ModelState.Remove("AssignedUser");

            if (ModelState.IsValid)
            {
                var currentUserRole = HttpContext.Session.GetString("UserRole");
                if (currentUserRole != UserRole.Leader.ToString())
                {
                    kpiTask.LeaderApprovedPoints = taskFromDb.LeaderApprovedPoints;
                }

                kpiTask.AssignedUserId = taskFromDb.AssignedUserId;
                kpiTask.CreatedDate = taskFromDb.CreatedDate;
                kpiTask.TaskDefinitionId = taskFromDb.TaskDefinitionId;
                kpiTask.UpdatedDate = System.DateTime.Now;
                kpiTask.RelatedDepartmentIds = string.Join(",", RelatedDepartmentIds);

                _context.Update(kpiTask);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật công việc thành công!";
                return RedirectToAction(nameof(Details), new { id = kpiTask.Id });
            }

            PopulateDropdowns(kpiTask);
            return View(kpiTask);
        }

        public async Task<IActionResult> ReviewList(int? employeeId, int? month, int? year)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != UserRole.Leader.ToString() && userRole != UserRole.Admin.ToString())
                return Forbid();

            int currentMonth = month ?? System.DateTime.Now.Month;
            int currentYear = year ?? System.DateTime.Now.Year;

            ViewData["EmployeeId"] = new SelectList(_context.Users.Where(u => u.Role == UserRole.Employee), "Id", "Name", employeeId);
            ViewData["SelectedMonth"] = currentMonth;
            ViewData["SelectedYear"] = currentYear;

            var query = _context.KpiTasks
                .Include(t => t.TaskDefinition)
                .Include(t => t.AssignedUser)
                .Where(t => t.CreatedDate.Year == currentYear && t.CreatedDate.Month == currentMonth);

            if (employeeId.HasValue)
            {
                query = query.Where(t => t.AssignedUserId == employeeId.Value);
            }

            var tasks = await query.OrderByDescending(t => t.CreatedDate).ToListAsync();
            return View(tasks);
        }
    }
}

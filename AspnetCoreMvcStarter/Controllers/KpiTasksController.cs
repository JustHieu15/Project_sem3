using AspnetCoreMvcStarter.Context;
using AspnetCoreMvcStarter.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

// Đặt alias để tránh xung đột với System.Threading.Tasks.TaskStatus
using TaskStatusEnum = AspnetCoreMvcStarter.Models.TaskStatus;

namespace AspnetCoreMvcStarter.Controllers
{
    [Authorize]
    public class KpiTasksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public KpiTasksController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            IQueryable<KpiTask> kpiTasksQuery = _context.KpiTasks
                .Include(k => k.AssignedUser)
                .Include(k => k.ApprovedByUser);

            if (User.IsInRole(nameof(UserRole.Admin)))
            {
                // Admin thấy tất cả
            }
            else if (User.IsInRole(nameof(UserRole.Leader)))
            {
                kpiTasksQuery = kpiTasksQuery.Where(t => t.AssignedUserId == currentUser.Id || t.AssignedUser.DepartmentId == currentUser.DepartmentId);
            }
            else
            {
                kpiTasksQuery = kpiTasksQuery.Where(t => t.AssignedUserId == currentUser.Id);
            }

            var kpiTasks = await kpiTasksQuery.OrderByDescending(t => t.CreatedDate).ToListAsync();
            return View(kpiTasks);
        }

        public async Task<IActionResult> MyTasks()
        {
          var currentUser = await _userManager.GetUserAsync(User);
          var kpiTasks = await _context.KpiTasks
            .Include(k => k.AssignedUser)
            .Include(k => k.ApprovedByUser)
            .Where(t => t.AssignedUserId == currentUser.Id)
            .OrderByDescending(t => t.CreatedDate)
            .ToListAsync();
          return View(kpiTasks);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var kpiTask = await _context.KpiTasks
                .Include(k => k.AssignedUser)
                .Include(k => k.ApprovedByUser)
                .Include(k => k.TaskUpdates)
                    .ThenInclude(u => u.UpdatedByUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (kpiTask == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (User.IsInRole(nameof(UserRole.Employee)) && kpiTask.AssignedUserId != currentUser.Id)
            {
                return Forbid();
            }

            return View(kpiTask);
        }

        public async Task<IActionResult> Create()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (User.IsInRole(nameof(UserRole.Leader)) || User.IsInRole(nameof(UserRole.Admin)))
            {
                ViewData["AssignedUserId"] = new SelectList(_context.Users, "Id", "Name");
            }
            else
            {
                ViewData["AssignedUserId"] = new SelectList(new List<User> { currentUser }, "Id", "Name", currentUser.Id);
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TaskContent,DocumentNumber,RequiredDays,StartDate,EndDate,ProposedPoints,ResponsibleLeader,AssignedUserId")] KpiTask kpiTask)
        {
            ModelState.Remove("AssignedUser");
            ModelState.Remove("ApprovedByUser");
            ModelState.Remove("TaskUpdates");

            if (ModelState.IsValid)
            {
                _context.Add(kpiTask);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (User.IsInRole(nameof(UserRole.Leader)) || User.IsInRole(nameof(UserRole.Admin)))
            {
                ViewData["AssignedUserId"] = new SelectList(_context.Users, "Id", "Name", kpiTask.AssignedUserId);
            }
            else
            {
                ViewData["AssignedUserId"] = new SelectList(new List<User> { currentUser }, "Id", "Name", kpiTask.AssignedUserId);
            }

            return View(kpiTask);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            var kpiTask = await _context.KpiTasks.FindAsync(id);
            return View(kpiTask);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            var kpiTask = await _context.KpiTasks.FindAsync(id);
            return View(kpiTask);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, TaskStatusEnum newStatus, string updateContent)
        {
            var task = await _context.KpiTasks.FindAsync(id);
            if (task == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);

            if (task.AssignedUserId == currentUser.Id)
            {
                if (newStatus == TaskStatusEnum.InProgress || newStatus == TaskStatusEnum.Completed)
                {
                    task.Status = newStatus;
                    task.UpdatedDate = DateTime.Now;

                    var taskUpdate = new TaskUpdate
                    {
                        TaskId = id,
                        UpdateContent = string.IsNullOrEmpty(updateContent) ? $"Chuyển trạng thái sang {newStatus}" : updateContent,
                        CompletionPercentage = (newStatus == TaskStatusEnum.Completed) ? 100 : 0,
                        UpdatedByUserId = currentUser.Id,
                    };
                    _context.TaskUpdates.Add(taskUpdate);

                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("Details", new { id = id });
        }

        [HttpPost]
        [Authorize(Roles = $"{nameof(UserRole.Leader)},{nameof(UserRole.Admin)}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, int approvedPoints)
        {
            var task = await _context.KpiTasks.FindAsync(id);
            if (task == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (task.ResponsibleLeader != currentUser.Name && !User.IsInRole(nameof(UserRole.Admin)))
            {
                return Forbid();
            }

            if (task.Status == TaskStatusEnum.Completed)
            {
                task.Status = TaskStatusEnum.Approved;
                task.ApprovedPoints = approvedPoints;
                task.ApprovedByUserId = currentUser.Id;
                task.UpdatedDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", new { id = id });
        }

        [HttpPost]
        [Authorize(Roles = $"{nameof(UserRole.Leader)},{nameof(UserRole.Admin)}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string notes)
        {
            var task = await _context.KpiTasks.FindAsync(id);
            if (task == null) return NotFound();

            if (task.Status == TaskStatusEnum.Completed)
            {
                task.Status = TaskStatusEnum.Rejected;
                task.Notes = notes;
                task.UpdatedDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Details", new { id = id });
        }
    }
}

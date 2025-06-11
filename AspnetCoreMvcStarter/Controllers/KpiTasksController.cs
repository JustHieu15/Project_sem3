using AspnetCoreMvcStarter.Context;
using AspnetCoreMvcStarter.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

// Đặt alias để tránh xung đột với System.Threading.Tasks.TaskStatus
using TaskStatusEnum = AspnetCoreMvcStarter.Models.TaskStatus;

namespace AspnetCoreMvcStarter.Controllers
{
    public class KpiTasksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KpiTasksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: KpiTasks - Chỉ Admin xem tất cả
        public async Task<IActionResult> Index()
        {
            var currentUserRole = HttpContext.Session.GetString("UserRole");
            var currentUserId = HttpContext.Session.GetInt32("UserId");

            if (currentUserRole != UserRole.Admin.ToString())
            {
                TempData["Error"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("MyTasks");
            }

            var kpiTasks = await _context.KpiTasks
                .Include(k => k.AssignedUser)
                .Include(k => k.ApprovedByUser)
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync();

            return View(kpiTasks);
        }

        // GET: KpiTasks/MyTasks - Công việc của tôi
        public async Task<IActionResult> MyTasks()
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            var currentUserRole = HttpContext.Session.GetString("UserRole");

            if (!currentUserId.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            IQueryable<KpiTask> kpiTasksQuery = _context.KpiTasks
                .Include(k => k.AssignedUser)
                .Include(k => k.ApprovedByUser);

            if (currentUserRole == UserRole.Employee.ToString())
            {
                // Nhân viên chỉ xem công việc được giao cho mình
                kpiTasksQuery = kpiTasksQuery.Where(t => t.AssignedUserId == currentUserId.Value);
            }
            else if (currentUserRole == UserRole.Leader.ToString())
            {
                // Lãnh đạo xem công việc của mình và của nhân viên cùng phòng ban
                var currentUser = await _context.Users.FindAsync(currentUserId.Value);
                kpiTasksQuery = kpiTasksQuery.Where(t =>
                    t.AssignedUserId == currentUserId.Value ||
                    (currentUser != null && t.AssignedUser.DepartmentId == currentUser.DepartmentId));
            }
            else // Admin
            {
                kpiTasksQuery = kpiTasksQuery.Where(t => t.AssignedUserId == currentUserId.Value);
            }

            var kpiTasks = await kpiTasksQuery
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync();

            return View(kpiTasks);
        }

        // GET: KpiTasks/Details/5
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

            var currentUserId = HttpContext.Session.GetInt32("UserId");
            var currentUserRole = HttpContext.Session.GetString("UserRole");

            // Kiểm tra quyền xem chi tiết
            if (currentUserRole == UserRole.Employee.ToString() &&
                kpiTask.AssignedUserId != currentUserId)
            {
                TempData["Error"] = "Bạn không có quyền xem công việc này.";
                return RedirectToAction("MyTasks");
            }

            return View(kpiTask);
        }

        // GET: KpiTasks/Create - Chỉ Leader và Admin
        public async Task<IActionResult> Create()
        {
            var currentUserRole = HttpContext.Session.GetString("UserRole");

            if (currentUserRole == UserRole.Employee.ToString())
            {
                TempData["Error"] = "Bạn không có quyền tạo công việc.";
                return RedirectToAction("MyTasks");
            }

            var currentUserId = HttpContext.Session.GetInt32("UserId");
            var currentUser = await _context.Users.FindAsync(currentUserId);

            if (currentUserRole == UserRole.Leader.ToString())
            {
                // Leader chỉ được giao việc cho nhân viên cùng phòng ban
                var departmentUsers = await _context.Users
                    .Where(u => u.DepartmentId == currentUser.DepartmentId && u.Role == UserRole.Employee)
                    .OrderBy(u => u.Name)
                    .ToListAsync();
                ViewData["AssignedUserId"] = new SelectList(departmentUsers, "Id", "Name");
            }
            else // Admin
            {
                var allUsers = await _context.Users
                    .OrderBy(u => u.Name)
                    .ToListAsync();
                ViewData["AssignedUserId"] = new SelectList(allUsers, "Id", "Name");
            }

            return View();
        }

        // POST: KpiTasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TaskContent,DocumentNumber,RequiredDays,StartDate,EndDate,ProposedPoints,ResponsibleLeader,AssignedUserId")] KpiTask kpiTask)
        {
            var currentUserRole = HttpContext.Session.GetString("UserRole");

            if (currentUserRole == UserRole.Employee.ToString())
            {
                TempData["Error"] = "Bạn không có quyền tạo công việc.";
                return RedirectToAction("MyTasks");
            }

            // Xóa các validation không cần thiết
            ModelState.Remove("AssignedUser");
            ModelState.Remove("ApprovedByUser");
            ModelState.Remove("TaskUpdates");

            if (ModelState.IsValid)
            {
                kpiTask.CreatedDate = DateTime.Now;
                kpiTask.Status = TaskStatusEnum.Pending;

                _context.Add(kpiTask);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Tạo công việc thành công!";
                return RedirectToAction(nameof(MyTasks));
            }

            // Reload SelectList nếu có lỗi
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            var currentUser = await _context.Users.FindAsync(currentUserId);

            if (currentUserRole == UserRole.Leader.ToString())
            {
                var departmentUsers = await _context.Users
                    .Where(u => u.DepartmentId == currentUser.DepartmentId && u.Role == UserRole.Employee)
                    .OrderBy(u => u.Name)
                    .ToListAsync();
                ViewData["AssignedUserId"] = new SelectList(departmentUsers, "Id", "Name", kpiTask.AssignedUserId);
            }
            else
            {
                var allUsers = await _context.Users
                    .OrderBy(u => u.Name)
                    .ToListAsync();
                ViewData["AssignedUserId"] = new SelectList(allUsers, "Id", "Name", kpiTask.AssignedUserId);
            }

            return View(kpiTask);
        }

        // GET: KpiTasks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var kpiTask = await _context.KpiTasks.FindAsync(id);
            if (kpiTask == null) return NotFound();

            var currentUserRole = HttpContext.Session.GetString("UserRole");
            var currentUserId = HttpContext.Session.GetInt32("UserId");

            // Chỉ Leader, Admin hoặc người được giao việc mới có thể sửa
            if (currentUserRole == UserRole.Employee.ToString() && kpiTask.AssignedUserId != currentUserId)
            {
                TempData["Error"] = "Bạn không có quyền chỉnh sửa công việc này.";
                return RedirectToAction("MyTasks");
            }

            // Load users cho dropdown
            if (currentUserRole != UserRole.Employee.ToString())
            {
                var currentUser = await _context.Users.FindAsync(currentUserId);
                if (currentUserRole == UserRole.Leader.ToString())
                {
                    var departmentUsers = await _context.Users
                        .Where(u => u.DepartmentId == currentUser.DepartmentId && u.Role == UserRole.Employee)
                        .OrderBy(u => u.Name)
                        .ToListAsync();
                    ViewData["AssignedUserId"] = new SelectList(departmentUsers, "Id", "Name", kpiTask.AssignedUserId);
                }
                else
                {
                    var allUsers = await _context.Users
                        .OrderBy(u => u.Name)
                        .ToListAsync();
                    ViewData["AssignedUserId"] = new SelectList(allUsers, "Id", "Name", kpiTask.AssignedUserId);
                }
            }

            return View(kpiTask);
        }

        // POST: KpiTasks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TaskContent,DocumentNumber,RequiredDays,StartDate,EndDate,ProposedPoints,ResponsibleLeader,AssignedUserId,Status,ApprovedPoints,Notes")] KpiTask kpiTask)
        {
            if (id != kpiTask.Id) return NotFound();

            var currentUserRole = HttpContext.Session.GetString("UserRole");
            var currentUserId = HttpContext.Session.GetInt32("UserId");

            // Kiểm tra quyền
            var existingTask = await _context.KpiTasks.FindAsync(id);
            if (existingTask == null) return NotFound();

            if (currentUserRole == UserRole.Employee.ToString() && existingTask.AssignedUserId != currentUserId)
            {
                TempData["Error"] = "Bạn không có quyền chỉnh sửa công việc này.";
                return RedirectToAction("MyTasks");
            }

            ModelState.Remove("AssignedUser");
            ModelState.Remove("ApprovedByUser");
            ModelState.Remove("TaskUpdates");

            if (ModelState.IsValid)
            {
                try
                {
                    // Nhân viên chỉ được cập nhật một số trường
                    if (currentUserRole == UserRole.Employee.ToString())
                    {
                        existingTask.Status = kpiTask.Status;
                        existingTask.Notes = kpiTask.Notes;
                        existingTask.UpdatedDate = DateTime.Now;
                    }
                    else
                    {
                        // Leader/Admin có thể cập nhật tất cả
                        existingTask.TaskContent = kpiTask.TaskContent;
                        existingTask.DocumentNumber = kpiTask.DocumentNumber;
                        existingTask.RequiredDays = kpiTask.RequiredDays;
                        existingTask.StartDate = kpiTask.StartDate;
                        existingTask.EndDate = kpiTask.EndDate;
                        existingTask.ProposedPoints = kpiTask.ProposedPoints;
                        existingTask.ResponsibleLeader = kpiTask.ResponsibleLeader;
                        existingTask.AssignedUserId = kpiTask.AssignedUserId;
                        existingTask.Status = kpiTask.Status;
                        existingTask.ApprovedPoints = kpiTask.ApprovedPoints;
                        existingTask.Notes = kpiTask.Notes;
                        existingTask.UpdatedDate = DateTime.Now;
                    }

                    _context.Update(existingTask);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Cập nhật công việc thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KpiTaskExists(kpiTask.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(MyTasks));
            }

            return View(kpiTask);
        }

        // GET: KpiTasks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var currentUserRole = HttpContext.Session.GetString("UserRole");

            if (currentUserRole == UserRole.Employee.ToString())
            {
                TempData["Error"] = "Bạn không có quyền xóa công việc.";
                return RedirectToAction("MyTasks");
            }

            var kpiTask = await _context.KpiTasks
                .Include(k => k.AssignedUser)
                .Include(k => k.ApprovedByUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (kpiTask == null) return NotFound();

            return View(kpiTask);
        }

        // POST: KpiTasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var currentUserRole = HttpContext.Session.GetString("UserRole");

            if (currentUserRole == UserRole.Employee.ToString())
            {
                TempData["Error"] = "Bạn không có quyền xóa công việc.";
                return RedirectToAction("MyTasks");
            }

            var kpiTask = await _context.KpiTasks.FindAsync(id);
            if (kpiTask != null)
            {
                _context.KpiTasks.Remove(kpiTask);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa công việc thành công!";
            }

            return RedirectToAction(nameof(MyTasks));
        }

        // POST: KpiTasks/UpdateStatus
        // POST: KpiTasks/UpdateTaskStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTaskStatus(
            int id,
            TaskStatusEnum newStatus,
            string updateContent,
            int completionPercentage = 0)
        {
            var task = await _context.KpiTasks.FindAsync(id);
            if (task == null) return NotFound();

            var currentUserId = HttpContext.Session.GetInt32("UserId");
            var currentUserRole = HttpContext.Session.GetString("UserRole");

            // Kiểm tra null cho currentUserId
            if (!currentUserId.HasValue)
            {
                TempData["Error"] = "Vui lòng đăng nhập lại";
                return RedirectToAction("Login", "Auth");
            }

            // Nhân viên chỉ có thể cập nhật công việc được giao cho mình
            if (currentUserRole == UserRole.Employee.ToString() && task.AssignedUserId != currentUserId)
            {
                TempData["Error"] = "Bạn không có quyền cập nhật công việc này.";
                return RedirectToAction("Details", new { id = id });
            }

            // Cập nhật trạng thái
            if (task.AssignedUserId == currentUserId || currentUserRole != UserRole.Employee.ToString())
            {
                task.Status = newStatus;
                task.UpdatedDate = DateTime.Now;

                // Tạo TaskUpdate
                var taskUpdate = new TaskUpdate
                {
                    TaskId = id,
                    UpdateContent = string.IsNullOrEmpty(updateContent)
                        ? $"Chuyển trạng thái sang {GetStatusDisplayName(newStatus)}"
                        : updateContent,
                    CompletionPercentage = completionPercentage,
                    UpdatedByUserId = currentUserId.Value,
                    UpdateDate = DateTime.Now,
                    UpdateStatus = UpdateStatus.Submitted
                };

                _context.TaskUpdates.Add(taskUpdate);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Cập nhật trạng thái thành công!";
            }

            return RedirectToAction("Details", new { id = id });
        }

        // POST: KpiTasks/Approve
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, int approvedPoints)
        {
            var currentUserRole = HttpContext.Session.GetString("UserRole");

            if (currentUserRole == UserRole.Employee.ToString())
            {
                TempData["Error"] = "Bạn không có quyền duyệt công việc.";
                return RedirectToAction("Details", new { id = id });
            }

            var task = await _context.KpiTasks.FindAsync(id);
            if (task == null) return NotFound();

            var currentUserId = HttpContext.Session.GetInt32("UserId");

            if (task.Status == TaskStatusEnum.Completed)
            {
                task.Status = TaskStatusEnum.Approved;
                task.ApprovedPoints = approvedPoints;
                task.ApprovedByUserId = currentUserId;
                task.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Duyệt công việc thành công!";
            }
            else
            {
                TempData["Error"] = "Chỉ có thể duyệt công việc đã hoàn thành.";
            }

            return RedirectToAction("Details", new { id = id });
        }

        // POST: KpiTasks/Reject
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string notes)
        {
            var currentUserRole = HttpContext.Session.GetString("UserRole");

            if (currentUserRole == UserRole.Employee.ToString())
            {
                TempData["Error"] = "Bạn không có quyền từ chối công việc.";
                return RedirectToAction("Details", new { id = id });
            }

            var task = await _context.KpiTasks.FindAsync(id);
            if (task == null) return NotFound();

            if (task.Status == TaskStatusEnum.Completed)
            {
                task.Status = TaskStatusEnum.Rejected;
                task.Notes = notes;
                task.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Từ chối công việc thành công!";
            }
            else
            {
                TempData["Error"] = "Chỉ có thể từ chối công việc đã hoàn thành.";
            }

            return RedirectToAction("Details", new { id = id });
        }

        private bool KpiTaskExists(int id)
        {
            return _context.KpiTasks.Any(e => e.Id == id);
        }

        private string GetStatusDisplayName(TaskStatusEnum status)
        {
            return status switch
            {
                TaskStatusEnum.Pending => "Chờ xử lý",
                TaskStatusEnum.InProgress => "Đang thực hiện",
                TaskStatusEnum.Completed => "Hoàn thành",
                TaskStatusEnum.Approved => "Đã duyệt",
                TaskStatusEnum.Rejected => "Từ chối",
                TaskStatusEnum.Overdue => "Quá hạn",
                _ => status.ToString()
            };
        }
    }
}

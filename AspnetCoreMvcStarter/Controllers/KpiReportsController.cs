using AspnetCoreMvcStarter.Context;
using AspnetCoreMvcStarter.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AspnetCoreMvcStarter.Controllers
{
    [Authorize]
    public class KpiReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KpiReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Helper method to get current user
        private async Task<User> GetCurrentUserAsync()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out int userId))
            {
                return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            }
            return null;
        }

        // Alternative helper method using session
        private async Task<User> GetCurrentUserFromSessionAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue)
            {
                return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
            }
            return null;
        }

        // GET: KpiReports
        public async Task<IActionResult> Index(int? month, int? year)
        {
            ViewData["CurrentMonth"] = month ?? DateTime.Now.Month;
            ViewData["CurrentYear"] = year ?? DateTime.Now.Year;

            var currentUser = await GetCurrentUserAsync() ?? await GetCurrentUserFromSessionAsync();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            IQueryable<MonthlyKpiReport> reportsQuery = _context.MonthlyKpiReports.Include(r => r.User);

            // Check user role from claims or session
            var userRole = User.FindFirstValue(ClaimTypes.Role) ?? HttpContext.Session.GetString("UserRole");

            if (userRole == UserRole.Admin.ToString())
            {
                // Admin sees all reports
            }
            else if (userRole == UserRole.Leader.ToString())
            {
                reportsQuery = reportsQuery.Where(r => r.UserId == currentUser.Id || r.User.DepartmentId == currentUser.DepartmentId);
            }
            else // Employee
            {
                reportsQuery = reportsQuery.Where(r => r.UserId == currentUser.Id);
            }

            var reports = await reportsQuery
                                .Where(r => r.Month == (month ?? DateTime.Now.Month) && r.Year == (year ?? DateTime.Now.Year))
                                .ToListAsync();
            return View(reports);
        }

        public async Task<IActionResult> MyReport(int? month, int? year)
        {
            var currentUser = await GetCurrentUserAsync() ?? await GetCurrentUserFromSessionAsync();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewData["CurrentMonth"] = month ?? DateTime.Now.Month;
            ViewData["CurrentYear"] = year ?? DateTime.Now.Year;

            var reports = await _context.MonthlyKpiReports
                .Include(r => r.User)
                .Where(r => r.UserId == currentUser.Id && r.Month == (month ?? DateTime.Now.Month) && r.Year == (year ?? DateTime.Now.Year))
                .ToListAsync();

            return View(reports);
        }

        // GET: KpiReports/Generate
        public async Task<IActionResult> Generate(int month, int year)
        {
            var users = await _context.Users.ToListAsync();

            foreach (var user in users)
            {
                bool reportExists = await _context.MonthlyKpiReports
                    .AnyAsync(r => r.UserId == user.Id && r.Month == month && r.Year == year);

                if (!reportExists)
                {
                    var tasksInMonth = await _context.KpiTasks
                        .Where(t => t.AssignedUserId == user.Id && t.EndDate.Year == year && t.EndDate.Month == month)
                        .ToListAsync();

                    if (tasksInMonth.Any())
                    {
                        int completedTasks = tasksInMonth.Count(t => t.Status == AspnetCoreMvcStarter.Models.TaskStatus.Approved);
                        int totalTasks = tasksInMonth.Count;

                        var newReport = new MonthlyKpiReport
                        {
                            Month = month,
                            Year = year,
                            UserId = user.Id,
                            TotalKpiPoints = tasksInMonth.Where(t => t.Status == AspnetCoreMvcStarter.Models.TaskStatus.Approved).Sum(t => t.ApprovedPoints ?? 0),
                            CompletedTasks = completedTasks,
                            PendingTasks = totalTasks - completedTasks,
                            CompletionRate = (totalTasks > 0) ? (decimal)completedTasks / totalTasks * 100 : 0
                        };
                        _context.MonthlyKpiReports.Add(newReport);
                    }
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { month, year });
        }
    }
}

using AspnetCoreMvcStarter.Context;
using AspnetCoreMvcStarter.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspnetCoreMvcStarter.Controllers
{
    [Authorize]
    public class KpiReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public KpiReportsController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: KpiReports
        public async Task<IActionResult> Index(int? month, int? year)
        {
            ViewData["CurrentMonth"] = month ?? DateTime.Now.Month;
            ViewData["CurrentYear"] = year ?? DateTime.Now.Year;

            var currentUser = await _userManager.GetUserAsync(User);

            IQueryable<MonthlyKpiReport> reportsQuery = _context.MonthlyKpiReports.Include(r => r.User);

            if (User.IsInRole(nameof(UserRole.Admin)))
            {
                // Admin sees all reports
            }
            else if (User.IsInRole(nameof(UserRole.Leader)))
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
          var currentUser = await _userManager.GetUserAsync(User);
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

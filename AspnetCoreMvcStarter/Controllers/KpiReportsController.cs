using AspnetCoreMvcStarter.Context;
using AspnetCoreMvcStarter.Models;
using AspnetCoreMvcStarter.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;

// Sử dụng alias để giải quyết xung đột tên
using TaskStatus = AspnetCoreMvcStarter.Models.TaskStatus;

namespace AspnetCoreMvcStarter.Controllers
{
    [Auth]
    public class KpiReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public KpiReportsController(ApplicationDbContext context) { _context = context; }

        public IActionResult Index()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == UserRole.Leader.ToString() || userRole == UserRole.Admin.ToString())
            {
                return RedirectToAction(nameof(TeamReport));
            }
            return RedirectToAction(nameof(MyReport));
        }

        public async Task<IActionResult> MyReport(int? month, int? year)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (!currentUserId.HasValue) return RedirectToAction("Login", "Auth");

            int currentMonth = month ?? System.DateTime.Now.Month;
            int currentYear = year ?? System.DateTime.Now.Year;
            ViewData["SelectedMonth"] = currentMonth;
            ViewData["SelectedYear"] = currentYear;

            var tasksInPeriod = await _context.KpiTasks
                .Where(t => t.AssignedUserId == currentUserId.Value &&
                            t.CreatedDate.Year == currentYear &&
                            t.CreatedDate.Month == currentMonth)
                .Include(t => t.TaskDefinition)
                .ToListAsync();

            return View(tasksInPeriod);
        }

        public async Task<IActionResult> TeamReport(int? month, int? year)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != UserRole.Leader.ToString() && userRole != UserRole.Admin.ToString())
                return Forbid();

            int currentMonth = month ?? System.DateTime.Now.Month;
            int currentYear = year ?? System.DateTime.Now.Year;
            ViewData["SelectedMonth"] = currentMonth;
            ViewData["SelectedYear"] = currentYear;

            var employees = await _context.Users
                .Where(u => u.Role == UserRole.Employee)
                .ToListAsync();

            var reportSummaries = new List<EmployeeReportSummary>();

            foreach (var emp in employees)
            {
                var tasks = _context.KpiTasks
                    .Where(t => t.AssignedUserId == emp.Id &&
                                t.CreatedDate.Year == currentYear &&
                                t.CreatedDate.Month == currentMonth);

                reportSummaries.Add(new EmployeeReportSummary
                {
                    UserId = emp.Id,
                    UserName = emp.Name,
                    TotalTasks = await tasks.CountAsync(),

                    // === LOGIC "ĐÃ DUYỆT" ĐÃ SỬA ===
                    // Đếm số task có điểm lãnh đạo chấm > 0
                    ApprovedTasks = await tasks.CountAsync(t => t.LeaderApprovedPoints.HasValue && t.LeaderApprovedPoints > 0),

                    InProgressTasks = await tasks.CountAsync(t => t.Status == TaskStatus.InProgress),

                    // Logic tính điểm vẫn giữ nguyên
                    TotalPoints = await tasks.Where(t => t.LeaderApprovedPoints.HasValue && t.LeaderApprovedPoints > 0)
                                            .SumAsync(t => t.LeaderApprovedPoints.Value)
                });
            }
            return View(reportSummaries);
        }

        public async Task<IActionResult> ExportTeamReportToExcel(int month, int year)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != UserRole.Leader.ToString() && userRole != UserRole.Admin.ToString()) return Forbid();

            var employees = await _context.Users.Where(u => u.Role == UserRole.Employee).ToListAsync();
            var reportData = new List<EmployeeReportSummary>();
            foreach (var emp in employees)
            {
                var tasks = _context.KpiTasks
                    .Where(t => t.AssignedUserId == emp.Id && t.CreatedDate.Year == year && t.CreatedDate.Month == month);

                reportData.Add(new EmployeeReportSummary
                {
                    UserId = emp.Id,
                    UserName = emp.Name,
                    TotalTasks = await tasks.CountAsync(),

                    // === LOGIC "ĐÃ DUYỆT" ĐÃ SỬA (đồng bộ) ===
                    ApprovedTasks = await tasks.CountAsync(t => t.LeaderApprovedPoints.HasValue && t.LeaderApprovedPoints > 0),

                    InProgressTasks = await tasks.CountAsync(t => t.Status == TaskStatus.InProgress),

                    TotalPoints = await tasks.Where(t => t.LeaderApprovedPoints.HasValue && t.LeaderApprovedPoints > 0)
                                             .SumAsync(t => t.LeaderApprovedPoints.Value)
                });
            }

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("BaoCaoNhom");

                var titleCell = worksheet.Cell("A1");
                titleCell.Value = $"BÁO CÁO NHÓM THÁNG {month}/{year}";
                worksheet.Range("A1:E1").Merge().Style.Font.SetBold().Font.SetFontSize(16).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                var headerRow = worksheet.Row(3);
                headerRow.Cell(1).Value = "Họ Tên";
                headerRow.Cell(2).Value = "Tổng CV";
                headerRow.Cell(3).Value = "Đã duyệt";
                headerRow.Cell(4).Value = "Đang làm";
                headerRow.Cell(5).Value = "Tổng Điểm";
                headerRow.Style.Font.SetBold();
                headerRow.Style.Fill.SetBackgroundColor(XLColor.LightGray);

                int currentRow = 4;
                foreach (var item in reportData)
                {
                    worksheet.Cell(currentRow, 1).Value = item.UserName;
                    worksheet.Cell(currentRow, 2).Value = item.TotalTasks;
                    worksheet.Cell(currentRow, 3).Value = item.ApprovedTasks;
                    worksheet.Cell(currentRow, 4).Value = item.InProgressTasks;
                    worksheet.Cell(currentRow, 5).Value = item.TotalPoints;
                    currentRow++;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    string excelName = $"BaoCaoNhom_{month}-{year}.xlsx";
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
                }
            }
        }

        public async Task<IActionResult> ExportMyReportToExcel(int month, int year)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (!currentUserId.HasValue) return Forbid();

            var tasksInPeriod = await _context.KpiTasks
                .Where(t => t.AssignedUserId == currentUserId.Value && t.CreatedDate.Year == year && t.CreatedDate.Month == month)
                .Include(t => t.TaskDefinition)
                .Include(t => t.MainDepartment)
                .Select(t => new {
                    TaskName = t.TaskDefinition.Name,
                    DetailedDescription = t.DetailedDescription,
                    MainDepartmentName = t.MainDepartment != null ? t.MainDepartment.Name : "N/A",
                    Status = t.Status.ToString(),
                    UserPoints = t.UserProposedPoints,
                    LeaderPoints = t.LeaderApprovedPoints
                })
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("BaoCaoCaNhan");

                var titleCell = worksheet.Cell("A1");
                titleCell.Value = $"BÁO CÁO CÁ NHÂN THÁNG {month}/{year}";
                worksheet.Range("A1:F1").Merge().Style.Font.SetBold().Font.SetFontSize(16).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                var headerRow = worksheet.Row(3);
                headerRow.Cell(1).Value = "Tên Công việc";
                headerRow.Cell(2).Value = "Mô tả chi tiết";
                headerRow.Cell(3).Value = "Đơn vị chủ trì";
                headerRow.Cell(4).Value = "Trạng thái";
                headerRow.Cell(5).Value = "Điểm tự chấm";
                headerRow.Cell(6).Value = "Điểm được duyệt";
                headerRow.Style.Font.SetBold();
                headerRow.Style.Fill.SetBackgroundColor(XLColor.LightGray);

                worksheet.Cell(4, 1).InsertData(tasksInPeriod);

                worksheet.Column(1).AdjustToContents();
                worksheet.Column(2).Width = 50;
                worksheet.Column(2).Style.Alignment.WrapText = true;
                worksheet.Column(3).AdjustToContents();
                worksheet.Column(4).AdjustToContents();
                worksheet.Column(5).AdjustToContents();
                worksheet.Column(6).AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    string excelName = $"BaoCaoCaNhan_{month}-{year}.xlsx";
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
                }
            }
        }
    }
}

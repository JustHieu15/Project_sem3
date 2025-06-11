using AspnetCoreMvcStarter.Context;
using AspnetCoreMvcStarter.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace AspnetCoreMvcStarter.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: User
        public async Task<IActionResult> Index()
        {
            var userRoleString = HttpContext.Session.GetString("UserRole");
            bool isAdmin = userRoleString == UserRole.Admin.ToString();

            IQueryable<User> usersQuery = _context.Users.Include(u => u.Department);

            if (!isAdmin)
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (!userId.HasValue)
                {
                    TempData["Error"] = "Phiên đăng nhập không hợp lệ.";
                    return RedirectToAction("Login", "Auth");
                }
                usersQuery = usersQuery.Where(u => u.Id == userId.Value);
            }

            return View(await usersQuery.ToListAsync());
        }

        // GET: User/Create
        public IActionResult Create()
        {
            var userRoleString = HttpContext.Session.GetString("UserRole");
            if (userRoleString != UserRole.Admin.ToString())
            {
                TempData["Error"] = "Bạn không có quyền thực hiện hành động này.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Departments = new SelectList(_context.Departments.OrderBy(d => d.Name), "Id", "Name");
            return View();
        }

        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Email,Password,Role,DepartmentId,Address,Phone")] User user)
        {
            var userRoleString = HttpContext.Session.GetString("UserRole");
            if (userRoleString != UserRole.Admin.ToString())
            {
                TempData["Error"] = "Bạn không có quyền thực hiện hành động này.";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra email trùng
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    user.Password = HashPassword(user.Password);
                    user.CreatedDate = DateTime.UtcNow;
                    user.IsActive = true;

                    _context.Add(user);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Tạo người dùng thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    TempData["Error"] = "Lỗi khi tạo người dùng. Vui lòng kiểm tra dữ liệu.";
                    // Log lỗi nếu cần: ex.InnerException?.Message
                }
            }

            ViewBag.Departments = new SelectList(_context.Departments.OrderBy(d => d.Name), "Id", "Name", user.DepartmentId);
            return View(user);
        }

        // GET: User/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var currentUserId = HttpContext.Session.GetInt32("UserId");
            var currentUserRoleString = HttpContext.Session.GetString("UserRole");

            if (currentUserRoleString != UserRole.Admin.ToString() && currentUserId != user.Id)
            {
                TempData["Error"] = "Bạn không có quyền chỉnh sửa thông tin người dùng này.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Departments = new SelectList(_context.Departments.OrderBy(d => d.Name), "Id", "Name", user.DepartmentId);
            user.Password = "";
            return View(user);
        }

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Email,Role,DepartmentId,Address,Phone,IsActive")] User user, string? newPassword)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            var currentUserId = HttpContext.Session.GetInt32("UserId");
            var currentUserRoleString = HttpContext.Session.GetString("UserRole");
            bool isAdmin = currentUserRoleString == UserRole.Admin.ToString();

            if (!isAdmin && currentUserId != user.Id)
            {
                TempData["Error"] = "Bạn không có quyền chỉnh sửa thông tin người dùng này.";
                return RedirectToAction(nameof(Index));
            }

            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                TempData["Error"] = "Không tìm thấy người dùng.";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra email trùng (nếu email thay đổi)
            if (user.Email != existingUser.Email && await _context.Users.AnyAsync(u => u.Email == user.Email && u.Id != user.Id))
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    existingUser.Name = user.Name;
                    existingUser.Email = user.Email;

                    if (isAdmin)
                    {
                        existingUser.Role = user.Role;
                        existingUser.IsActive = user.IsActive;
                    }

                    existingUser.DepartmentId = user.DepartmentId;
                    existingUser.Address = user.Address;
                    existingUser.Phone = user.Phone;

                    if (!string.IsNullOrEmpty(newPassword))
                    {
                        existingUser.Password = HashPassword(newPassword);
                    }

                    _context.Update(existingUser);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật người dùng thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    TempData["Error"] = "Lỗi khi cập nhật người dùng. Vui lòng kiểm tra dữ liệu.";
                    // Log lỗi nếu cần: ex.InnerException?.Message
                }
            }

            ViewBag.Departments = new SelectList(_context.Departments.OrderBy(d => d.Name), "Id", "Name", user.DepartmentId);
            user.Password = "";
            return View(user);
        }

        // GET: User/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var userRoleString = HttpContext.Session.GetString("UserRole");
            if (userRoleString != UserRole.Admin.ToString())
            {
                TempData["Error"] = "Bạn không có quyền thực hiện hành động này.";
                return RedirectToAction(nameof(Index));
            }

            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userRoleString = HttpContext.Session.GetString("UserRole");
            if (userRoleString != UserRole.Admin.ToString())
            {
                TempData["Error"] = "Bạn không có quyền thực hiện hành động này.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy người dùng để xóa.";
                return RedirectToAction(nameof(Index));
            }

            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (user.Id == currentUserId && user.Role == UserRole.Admin)
            {
                TempData["Error"] = "Không thể xóa tài khoản admin hiện tại.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa người dùng thành công!";
            }
            catch (DbUpdateException ex)
            {
                TempData["Error"] = "Lỗi khi xóa người dùng, có thể do người dùng đang liên quan đến dữ liệu khác.";
                // Log lỗi nếu cần: ex.InnerException?.Message
            }

            return RedirectToAction(nameof(Index));
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}

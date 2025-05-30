using AspnetCoreMvcStarter.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using AspnetCoreMvcStarter.Context;
using Microsoft.AspNetCore.Mvc.Rendering; // Cần cho SelectList
using Microsoft.EntityFrameworkCore; // Cần cho ToListAsync hoặc Include nếu cần

namespace AspnetCoreMvcStarter.Controllers
{
    public class UserController : Controller // Nên áp dụng [Auth] ở đây nếu chưa làm toàn cục
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: User
        public async Task<IActionResult> Index()
        {
            // Lấy vai trò của người dùng từ Session
            var userRoleString = HttpContext.Session.GetString("UserRole");
            bool isAdmin = userRoleString == UserRole.Admin.ToString();

            IQueryable<User> usersQuery = _context.Users.Include(u => u.Department);

            if (!isAdmin) // Nếu không phải admin, chỉ hiển thị thông tin của chính họ
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId.HasValue)
                {
                    usersQuery = usersQuery.Where(u => u.Id == userId.Value);
                }
                else
                {
                    return View(new List<User>()); // Hoặc xử lý lỗi nếu UserId không có
                }
            }
            // Admin có thể xem tất cả người dùng
            return View(await usersQuery.ToListAsync());
        }


        // GET: User/Create
        public IActionResult Create()
        {
            // Kiểm tra quyền Admin
            var userRoleString = HttpContext.Session.GetString("UserRole");
            if (userRoleString != UserRole.Admin.ToString())
            {
                TempData["Error"] = "Bạn không có quyền thực hiện hành động này.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Departments = new SelectList(_context.Departments.OrderBy(d => d.Name).ToList(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Email,Password,Role,DepartmentId,Address,Phone")] User user)
        {
            // Kiểm tra quyền Admin
            var userRoleString = HttpContext.Session.GetString("UserRole");
            if (userRoleString != UserRole.Admin.ToString())
            {
                TempData["Error"] = "Bạn không có quyền thực hiện hành động này.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(user.Password))
                {
                    user.Password = HashPassword(user.Password);
                }
                user.CreatedDate = DateTime.UtcNow;
                user.IsActive = true;

                _context.Add(user);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Tạo người dùng thành công!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Departments = new SelectList(_context.Departments.OrderBy(d => d.Name).ToList(), "Id", "Name", user.DepartmentId);
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

            // Kiểm tra quyền: Admin có thể sửa tất cả, người dùng thường chỉ sửa thông tin của chính họ
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            var currentUserRoleString = HttpContext.Session.GetString("UserRole");

            if (currentUserRoleString != UserRole.Admin.ToString() && currentUserId != user.Id)
            {
                TempData["Error"] = "Bạn không có quyền chỉnh sửa thông tin người dùng này.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Departments = new SelectList(_context.Departments.OrderBy(d => d.Name).ToList(), "Id", "Name", user.DepartmentId);
            // Không truyền mật khẩu ra view, để trống để người dùng có thể thay đổi nếu muốn
            user.Password = "";
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Email,Role,DepartmentId,Address,Phone,IsActive")] User user, string? newPassword)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            // Kiểm tra quyền
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            var currentUserRoleString = HttpContext.Session.GetString("UserRole");
            bool isAdmin = currentUserRoleString == UserRole.Admin.ToString();

            if (!isAdmin && currentUserId != user.Id)
            {
                TempData["Error"] = "Bạn không có quyền chỉnh sửa thông tin người dùng này.";
                return RedirectToAction(nameof(Index));
            }

            // Lấy user hiện tại từ DB để chỉ cập nhật các trường cần thiết và không mất các trường không bind
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                TempData["Error"] = "Không tìm thấy người dùng.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    existingUser.Name = user.Name;
                    existingUser.Email = user.Email; // Cần cẩn thận nếu Email là duy nhất và không cho phép trùng

                    // Chỉ Admin mới được thay đổi Role và IsActive
                    if (isAdmin)
                    {
                        existingUser.Role = user.Role;
                        existingUser.IsActive = user.IsActive;
                    }
                    // Nếu không phải admin, Role và IsActive sẽ không được cập nhật từ form
                    // Mà sẽ giữ nguyên giá trị hiện tại của existingUser

                    existingUser.DepartmentId = user.DepartmentId;
                    existingUser.Address = user.Address;
                    existingUser.Phone = user.Phone;

                    if (!string.IsNullOrEmpty(newPassword)) // Nếu có nhập mật khẩu mới
                    {
                        existingUser.Password = HashPassword(newPassword);
                    }
                    // CreatedDate không thay đổi

                    _context.Update(existingUser);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật người dùng thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Users.Any(e => e.Id == user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            // Nếu ModelState không hợp lệ, cần load lại danh sách phòng ban
            ViewBag.Departments = new SelectList(_context.Departments.OrderBy(d => d.Name).ToList(), "Id", "Name", user.DepartmentId);
            // Để tránh lỗi validation password nếu người dùng không muốn đổi, trả về user từ DB
            user.Password = ""; // Hoặc existingUser.Password nếu bạn muốn giữ lại (nhưng không nên hiển thị)
            return View(user); // Hoặc return View(existingUser) nếu bạn muốn giữ lại các giá trị cũ khi có lỗi
        }


        // GET: User/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            // Kiểm tra quyền Admin
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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Kiểm tra quyền Admin
            var userRoleString = HttpContext.Session.GetString("UserRole");
            if (userRoleString != UserRole.Admin.ToString())
            {
                TempData["Error"] = "Bạn không có quyền thực hiện hành động này.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                // Kiểm tra không cho admin tự xóa chính mình (nếu cần)
                var currentUserId = HttpContext.Session.GetInt32("UserId");
                if(user.Id == currentUserId && user.Role == UserRole.Admin)
                {
                    TempData["Error"] = "Không thể xóa tài khoản admin hiện tại.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa người dùng thành công!";
            }
            else
            {
                TempData["Error"] = "Không tìm thấy người dùng để xóa.";
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

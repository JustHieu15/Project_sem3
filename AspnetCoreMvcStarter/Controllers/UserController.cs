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

        public async Task<IActionResult> Index()
        {
          var userRoleString = HttpContext.Session.GetString("UserRole");
          bool isAdmin = userRoleString == UserRole.Admin.ToString();

          IQueryable<User> usersQuery;

          if (isAdmin)
          {
            // Nếu là Admin, lấy tất cả người dùng và thông tin phòng ban của họ
            usersQuery = _context.Users
              .Include(u => u.Department) // <-- ĐÂY LÀ DÒNG QUAN TRỌNG NHẤT
              .OrderByDescending(u => u.Id);
          }
          else
          {
            // Nếu không phải Admin, chỉ lấy thông tin của chính người dùng đó
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
              // Xử lý trường hợp không tìm thấy UserId trong session
              TempData["Error"] = "Phiên đăng nhập không hợp lệ.";
              // Có thể chuyển hướng đến trang đăng nhập
              return RedirectToAction("Login", "Auth");
            }

            usersQuery = _context.Users
              .Include(u => u.Department) // <-- Vẫn cần Include ở đây
              .Where(u => u.Id == userId.Value);
          }

          // Thực thi truy vấn và gửi danh sách đến View
          var users = await usersQuery.ToListAsync();
          return View(users);
        }

        // === PHƯƠNG THỨC HỖ TRỢ ĐỂ TẠO DROPDOWN CHO PHÒNG BAN ===
        // Giúp tránh lặp code và xử lý việc chọn lại giá trị đã chọn khi có lỗi
        private void PopulateDepartmentsDropDownList(object selectedDepartment = null)
        {
            var departmentsQuery = from d in _context.Departments
                                   orderby d.Name
                                   select d;

            var departmentList = departmentsQuery.Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.Name
            }).ToList();

            // Thêm lựa chọn mặc định vào đầu danh sách
            departmentList.Insert(0, new SelectListItem { Value = "", Text = "-- Chọn phòng ban (Tùy chọn) --" });

            ViewBag.Departments = new SelectList(departmentList, "Value", "Text", selectedDepartment);
        }

        // GET: User/Create
        public IActionResult Create()
        {
          // === DÒNG CODE QUAN TRỌNG ĐỂ SỬA LỖI ===
          // Xóa tất cả các lỗi có thể tồn tại trong ModelState từ các request trước
          // hoặc từ các middleware/filter khác.
          ModelState.Clear();
          // =======================================

          var userRoleString = HttpContext.Session.GetString("UserRole");
          if (userRoleString != UserRole.Admin.ToString())
          {
            TempData["Error"] = "Bạn không có quyền thực hiện hành động này.";
            return RedirectToAction(nameof(Index));
          }

          // Xóa TempData cũ để không hiển thị lỗi từ các hành động trước đó
          TempData.Remove("Error");
          TempData.Remove("Success");

          // Gọi phương thức hỗ trợ để tạo dropdown
          PopulateDepartmentsDropDownList();

          // Truyền vào một model User mới để tránh lỗi NullReference trên view
          return View(new User());
        }

        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            var userRoleString = HttpContext.Session.GetString("UserRole");
            if (userRoleString != UserRole.Admin.ToString())
            {
                TempData["Error"] = "Bạn không có quyền thực hiện hành động này.";
                return RedirectToAction(nameof(Index));
            }

            // Xử lý DepartmentId: nếu là 0 thì set thành null
            if (user.DepartmentId == 0)
            {
                user.DepartmentId = null;
            }

            // Kiểm tra email trùng
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng.");
            }

            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["Error"] = "Vui lòng kiểm tra lại thông tin đã nhập: " + string.Join("; ", errorMessages);

                // Khi có lỗi, gọi lại phương thức hỗ trợ và truyền vào DepartmentId đã chọn
                // để dropdown hiển thị lại đúng giá trị người dùng đã chọn
                PopulateDepartmentsDropDownList(user.DepartmentId);

                return View(user);
            }

            try
            {
                user.Password = HashPassword(user.Password);
                user.CreatedDate = DateTime.Now;
                user.IsActive = true;

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Tạo người dùng mới thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi tạo người dùng: " + ex.Message;
                // Gọi lại phương thức hỗ trợ khi có lỗi ở DB
                PopulateDepartmentsDropDownList(user.DepartmentId);
                return View(user);
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            // === THÊM 2 DÒNG NÀY VÀO ĐÂY ===
            TempData.Remove("Error");
            TempData.Remove("Success");
            // ===============================

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            // Bạn có thể thêm code kiểm tra quyền ở đây nếu muốn
            // var currentUserId = ...
            // if (!isAdmin && currentUserId != user.Id) ...

            // Gọi phương thức hỗ trợ để tạo dropdown
            PopulateDepartmentsDropDownList(user.DepartmentId);

            // Không hiển thị hash password cũ ra view
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

            // Chúng ta không cần kiểm tra quyền ở đây vì logic cập nhật sẽ làm việc đó
            // tuy nhiên vẫn giữ để bảo mật sớm
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            var currentUserRoleString = HttpContext.Session.GetString("UserRole");
            bool isAdmin = currentUserRoleString == UserRole.Admin.ToString();

            if (!isAdmin && currentUserId != user.Id)
            {
                TempData["Error"] = "Bạn không có quyền chỉnh sửa thông tin người dùng này.";
                return RedirectToAction(nameof(Index));
            }

            // Bỏ qua lỗi validation của trường Password trong model trước
            // vì chúng ta sẽ xử lý nó thủ công.
            ModelState.Remove("Password");

            if (ModelState.IsValid)
            {
                // Lấy đối tượng gốc từ DB để cập nhật
                var existingUser = await _context.Users.FindAsync(id);
                if (existingUser == null)
                {
                    return NotFound();
                }

                // Cập nhật các trường thông thường
                existingUser.Name = user.Name;
                existingUser.Address = user.Address;
                existingUser.Phone = user.Phone;
                existingUser.DepartmentId = user.DepartmentId == 0 ? null : user.DepartmentId;

                // Chỉ Admin mới được cập nhật các trường này
                if (isAdmin)
                {
                    existingUser.Role = user.Role;
                    existingUser.IsActive = user.IsActive;
                }

                // Kiểm tra và cập nhật Email nếu nó thay đổi và hợp lệ
                if (existingUser.Email != user.Email)
                {
                    if (await _context.Users.AnyAsync(u => u.Email == user.Email && u.Id != id))
                    {
                        ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                    }
                    else
                    {
                        existingUser.Email = user.Email;
                    }
                }

                // Cập nhật mật khẩu nếu có mật khẩu mới được cung cấp
                if (!string.IsNullOrWhiteSpace(newPassword))
                {
                    if (newPassword.Length < 6)
                    {
                        ModelState.AddModelError("newPassword", "Mật khẩu mới phải có ít nhất 6 ký tự.");
                    }
                    else
                    {
                        existingUser.Password = HashPassword(newPassword);
                    }
                }

                // Kiểm tra lại ModelState một lần nữa sau khi đã thêm các lỗi thủ công
                if (ModelState.IsValid)
                {
                    try
                    {
                        await _context.SaveChangesAsync();
                        TempData["Success"] = "Cập nhật người dùng thành công!";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateException ex)
                    {
                        TempData["Error"] = "Lỗi khi cập nhật người dùng: " + (ex.InnerException?.Message ?? ex.Message);
                    }
                }
            }

            // Nếu có lỗi, nạp lại dropdown và hiển thị lại view
            var errorMessages = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            TempData["Error"] = "Vui lòng kiểm tra lại thông tin đã nhập: " + string.Join("; ", errorMessages);
            PopulateDepartmentsDropDownList(user.DepartmentId);

            // Để password trống trên view khi trả về
            user.Password = "";
            return View(user);
        }

        // ... (Code cho Delete và HashPassword giữ nguyên) ...

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

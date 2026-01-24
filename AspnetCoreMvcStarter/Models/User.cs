using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
// THÊM DÒNG NÀY VÀO
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AspnetCoreMvcStarter.Models
{
  public class User
  {
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Họ và tên là bắt buộc")]
    [StringLength(100)]
    [Display(Name = "Họ và tên")]
    public string Name { get; set; }

    [StringLength(200)]
    [Display(Name = "Địa chỉ")]
    public string Address { get; set; }

    [StringLength(15)]
    [Display(Name = "Số điện thoại")]
    public string Phone { get; set; }

    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [Display(Name = "Email")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
    [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
    [Display(Name = "Mật khẩu")]
    public string Password { get; set; }

    [Display(Name = "Phòng ban")]
    public int? DepartmentId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn chức vụ")]
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn chức vụ hợp lệ")]
    [Display(Name = "Vai trò")]
    public UserRole Role { get; set; } = UserRole.None;

    [Display(Name = "Ngày tạo tài khoản")]
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    [Display(Name = "Trạng thái tài khoản")]
    public bool IsActive { get; set; } = true;

    // Navigation Properties
    [ForeignKey("DepartmentId")]
    // === THÊM ATTRIBUTE NÀY VÀO ĐÂY ===
    [ValidateNever]
    public virtual Department Department { get; set; }

    // === CÁC THUỘC TÍNH KHÁC CŨNG NÊN CÓ VALIDATENEVER ĐỂ TRÁNH LỖI TƯƠNG TỰ ===
    [ValidateNever]
    public virtual ICollection<KpiTask> AssignedTasks { get; set; } = new List<KpiTask>();
    [ValidateNever]
    public virtual ICollection<KpiTask> ApprovedTasks { get; set; } = new List<KpiTask>();
    [ValidateNever]
    public virtual ICollection<TaskUpdate> TaskUpdates { get; set; } = new List<TaskUpdate>();
    [ValidateNever]
    public virtual ICollection<MonthlyKpiReport> MonthlyReports { get; set; } = new List<MonthlyKpiReport>();
  }
}

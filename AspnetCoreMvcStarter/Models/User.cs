using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspnetCoreMvcStarter.Models
{
  public class User
  {
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Họ và tên")]
    public string Name { get; set; }

    [StringLength(200)]
    [Display(Name = "Địa chỉ")]
    public string Address { get; set; }

    [StringLength(15)]
    [Display(Name = "Số điện thoại")]
    public string Phone { get; set; }

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; }

    [Display(Name = "Mật khẩu")]
    public string Password { get; set; }

    [Display(Name = "Phòng ban")]
    public int? DepartmentId { get; set; }

    [Display(Name = "Ngày tạo tài khoản")]
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    [Display(Name = "Trạng thái tài khoản")]
    public bool IsActive { get; set; } = true;

    // Navigation Properties
    [ForeignKey("DepartmentId")]
    public virtual Department Department { get; set; }

    public virtual ICollection<KpiTask> AssignedTasks { get; set; } = new List<KpiTask>();
    public virtual ICollection<KpiTask> ApprovedTasks { get; set; } = new List<KpiTask>();
    public virtual ICollection<TaskUpdate> TaskUpdates { get; set; } = new List<TaskUpdate>();
    public virtual ICollection<MonthlyKpiReport> MonthlyReports { get; set; } = new List<MonthlyKpiReport>();
  }
}

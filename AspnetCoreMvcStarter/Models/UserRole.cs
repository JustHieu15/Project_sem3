using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspnetCoreMvcStarter.Models
{
  public enum UserRole
  {
    [Display(Name="Chọn chức vụ")]
    None = 0,

    [Display(Name="Chuyên viên")]
    Employee = 1,

    [Display(Name="Lãnh đạo")]
    Leader = 2,

    [Display(Name="Quản trị viên")]
    Admin = 3
  }
}

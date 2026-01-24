using System.ComponentModel.DataAnnotations;

namespace AspnetCoreMvcStarter.Models
{
  public enum Priority
  {
    [Display(Name = "Thấp")]
    Low = 1,

    [Display(Name = "Bình thường")]
    Normal = 2,

    [Display(Name = "Cao")]
    High = 3,

    [Display(Name = "Khẩn cấp")]
    Urgent = 4,

    [Display(Name = "Rất khẩn cấp")]
    Critical = 5
  }
}

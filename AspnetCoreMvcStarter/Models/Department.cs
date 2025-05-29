using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspnetCoreMvcStarter.Models
{
  public class Department
  {
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Tên phòng ban")]
    public string Name { get; set; }

    [StringLength(50)]
    [Display(Name = "Mã phòng ban")]
    public string Code { get; set; }

    [StringLength(500)]
    [Display(Name = "Mô tả")]
    public string? Description { get; set; }

    [Display(Name = "Ngày tạo")]
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    // Navigation Properties
    public virtual ICollection<User> Users { get; set; } = new List<User>();
  }
}

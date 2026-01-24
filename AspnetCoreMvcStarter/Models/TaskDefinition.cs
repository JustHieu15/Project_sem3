// Models/TaskDefinition.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspnetCoreMvcStarter.Models
{
  // Model này đại diện cho một CÔNG VIỆC MẪU do Admin tạo ra
  public class TaskDefinition
  {
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên công việc là bắt buộc")]
    [StringLength(200)]
    [Display(Name = "Tên Công Việc")]
    public string Name { get; set; }

    [StringLength(500)]
    [Display(Name = "Mô tả")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn nhóm công việc")]
    [Display(Name = "Nhóm công việc")]
    public int JobTypeId { get; set; }

    [Display(Name = "Điểm đề xuất mặc định")]
    [Range(0, 100)]
    public int DefaultPoints { get; set; } = 5;

    [Display(Name = "Trạng thái")]
    public bool IsActive { get; set; } = true;

    // Navigation Properties
    [ForeignKey("JobTypeId")]
    public virtual JobType JobType { get; set; }

    public virtual ICollection<KpiTask> KpiTasks { get; set; } = new List<KpiTask>();
  }
}

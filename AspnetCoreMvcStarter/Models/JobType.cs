// Models/JobType.cs
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace AspnetCoreMvcStarter.Models
{
  public class JobType
  {
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên loại công việc là bắt buộc")]
    [StringLength(100)]
    [Display(Name = "Tên loại công việc")]
    public string Name { get; set; }

    [StringLength(20)]
    [Display(Name = "Mã loại")]
    public string? Code { get; set; }

    [StringLength(300)]
    [Display(Name = "Mô tả")]
    public string? Description { get; set; }

    [Display(Name = "Màu sắc hiển thị")]
    [StringLength(7)] // Để lưu mã màu hex (#FFFFFF)
    public string? Color { get; set; } = "#007bff";

    [Display(Name = "Điểm KPI mặc định")]
    [Range(1, 100)]
    public int DefaultKpiPoints { get; set; } = 10;

    [Display(Name = "Số ngày hoàn thành mặc định")]
    [Range(1, 365)]
    public int DefaultRequiredDays { get; set; } = 7;

    [Display(Name = "Độ ưu tiên")]
    [Range(1, 5)] // 1: Thấp, 2: Bình thường, 3: Cao, 4: Khẩn cấp, 5: Rất khẩn cấp
    public int Priority { get; set; } = 2;

    [Display(Name = "Trạng thái hoạt động")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Ngày tạo")]
    public System.DateTime CreatedDate { get; set; } = System.DateTime.Now;

    [Display(Name = "Thứ tự sắp xếp")]
    public int DisplayOrder { get; set; } = 0;

    // Navigation Properties
    public virtual ICollection<KpiTask> KpiTasks { get; set; } = new List<KpiTask>();

    // MỚI: Thêm quan hệ với danh mục công việc
    public virtual ICollection<TaskDefinition> TaskDefinitions { get; set; } = new List<TaskDefinition>();
  }
}

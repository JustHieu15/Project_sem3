using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspnetCoreMvcStarter.Models
{
  public class MonthlyKpiReport
  {
    [Key]
    public int Id { get; set; }

    [Required]
    [Display(Name = "Tháng")]
    [Range(1, 12)]
    public int Month { get; set; }

    [Required]
    [Display(Name = "Năm")]
    public int Year { get; set; }

    [Required]
    [Display(Name = "Nhân viên")]
    public int UserId { get; set; }

    [Display(Name = "Tổng điểm KPI")]
    public int TotalKpiPoints { get; set; }

    [Display(Name = "Số công việc hoàn thành")]
    public int CompletedTasks { get; set; }

    [Display(Name = "Số công việc chưa hoàn thành")]
    public int PendingTasks { get; set; }

    [Display(Name = "Tỷ lệ hoàn thành (%)")]
    public decimal CompletionRate { get; set; }

    [Display(Name = "Ngày tạo báo cáo")]
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    // Navigation Properties
    [ForeignKey("UserId")]
    public virtual User User { get; set; }
  }
}

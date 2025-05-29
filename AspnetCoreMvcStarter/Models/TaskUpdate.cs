using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspnetCoreMvcStarter.Models
{
  public class TaskUpdate
  {
    [Key]
    public int Id { get; set; }

    [Required]
    public int TaskId { get; set; }

    [Required]
    [StringLength(1000)]
    [Display(Name = "Nội dung cập nhật")]
    public string UpdateContent { get; set; }

    [Display(Name = "Phần trăm hoàn thành")]
    [Range(0, 100)]
    public int CompletionPercentage { get; set; }

    [Display(Name = "Ngày cập nhật")]
    public DateTime UpdateDate { get; set; } = DateTime.Now;

    [Required]
    [Display(Name = "Người cập nhật")]
    public int UpdatedByUserId { get; set; }

    [Display(Name = "Trạng thái cập nhật")]
    public UpdateStatus UpdateStatus { get; set; } = UpdateStatus.Submitted;

    // Navigation Properties
    [ForeignKey("TaskId")]
    public virtual KpiTask Task { get; set; }

    [ForeignKey("UpdatedByUserId")]
    public virtual User UpdatedByUser { get; set; }
  }
}

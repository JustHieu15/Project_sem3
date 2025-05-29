using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspnetCoreMvcStarter.Models
{
  public class KpiTask
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Nội dung công việc")]
        public string TaskContent { get; set; }

        [Required]
        [Display(Name = "Số văn bản/ngày Email giao nhiệm vụ")]
        public int DocumentNumber { get; set; }

        [Required]
        [Display(Name = "Số ngày yêu cầu hoàn thành")]
        public int RequiredDays { get; set; }

        [Required]
        [Display(Name = "Thời gian bắt đầu")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "Thời gian kết thúc")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required]
        [Display(Name = "Số điểm đề xuất")]
        public int ProposedPoints { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Lãnh đạo phụ trách")]
        public string ResponsibleLeader { get; set; }

        [Display(Name = "Trạng thái")]
        public TaskStatus Status { get; set; } = TaskStatus.Pending;

        [Display(Name = "Điểm KPI được duyệt")]
        public int? ApprovedPoints { get; set; }

        [Display(Name = "Ghi chú")]
        [StringLength(500)]
        public string? Notes { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedDate { get; set; }

        // Foreign Key
        [Required]
        [Display(Name = "Người thực hiện")]
        public int AssignedUserId { get; set; }

        [Display(Name = "Người duyệt")]
        public int? ApprovedByUserId { get; set; }

        // Navigation Properties
        [ForeignKey("AssignedUserId")]
        public virtual User AssignedUser { get; set; }

        [ForeignKey("ApprovedByUserId")]
        public virtual User? ApprovedByUser { get; set; }

        // Relationship with task updates
        public virtual ICollection<TaskUpdate> TaskUpdates { get; set; } = new List<TaskUpdate>();
    }
}

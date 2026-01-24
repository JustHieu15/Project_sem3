// Models/KpiTask.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace AspnetCoreMvcStarter.Models
{
    public class KpiTask
    {
        [Key]
        public int Id { get; set; }

        // --- ĐÃ THAY ĐỔI: LIÊN KẾT TỚI CÔNG VIỆC MẪU ---
        [Required(ErrorMessage = "Vui lòng chọn một công việc")]
        [Display(Name = "Công việc")]
        public int TaskDefinitionId { get; set; }

        // --- CÁC TRƯỜNG CHI TIẾT DO CHUYÊN VIÊN NHẬP - GIỮ NGUYÊN ---
        [StringLength(1000)]
        [Display(Name = "Mô tả chi tiết công việc (*)")]
        public string? DetailedDescription { get; set; }

        [Display(Name = "Người thực hiện")]
        public int? AssignedUserId { get; set; }

        [Display(Name = "Lãnh đạo duyệt (*)")]
        public int? ApproverId { get; set; }

        [Display(Name = "Điểm tự chấm")]
        [Range(0, 100, ErrorMessage = "Điểm phải từ 0 đến 100")]
        public int? UserProposedPoints { get; set; }

        [Display(Name = "Điểm được duyệt (Lãnh đạo)")]
        [Range(0, 100, ErrorMessage = "Điểm phải từ 0 đến 100")]
        public int? LeaderApprovedPoints { get; set; }

        [Display(Name = "Hạn HT giao trong VB")]
        [DataType(DataType.Date)]
        public DateTime? DeadlineInDocument { get; set; }

        [Display(Name = "Kết quả công việc (*)")]
        public TaskStatus Status { get; set; } = TaskStatus.InProgress;

        [StringLength(1000)]
        [Display(Name = "Lý do chưa hoàn thành (*)")]
        public string? ReasonForIncompletion { get; set; }

        [Display(Name = "Đơn vị chủ trì (*)")]
        public int? MainDepartmentId { get; set; }

        [Display(Name = "Đơn vị liên quan")]
        public string? RelatedDepartmentIds { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedDate { get; set; }

        // --- NAVIGATION PROPERTIES ---
        [ForeignKey("TaskDefinitionId")]
        public virtual TaskDefinition TaskDefinition { get; set; }

        [ForeignKey("AssignedUserId")]
        public virtual User? AssignedUser { get; set; }

        [ForeignKey("ApproverId")]
        public virtual User? Approver { get; set; }

        [ForeignKey("MainDepartmentId")]
        public virtual Department? MainDepartment { get; set; }

        public virtual ICollection<TaskUpdate> TaskUpdates { get; set; } = new List<TaskUpdate>();
    }
}

namespace DefaultNamespace
{
  public enum TaskStatus
  {
    [Display(Name = "Chờ xử lý")]
    Pending = 0,

    [Display(Name = "Đang thực hiện")]
    InProgress = 1,

    [Display(Name = "Hoàn thành")]
    Completed = 2,

    [Display(Name = "Đã duyệt")]
    Approved = 3,

    [Display(Name = "Từ chối")]
    Rejected = 4,

    [Display(Name = "Quá hạn")]
    Overdue = 5
  }
}

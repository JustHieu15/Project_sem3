namespace DefaultNamespace
{
  public enum UpdateStatus
  {
    [Display(Name = "Đã gửi")]
    Submitted = 0,

    [Display(Name = "Đã xem")]
    Reviewed = 1,

    [Display(Name = "Đã duyệt")]
    Approved = 2,

    [Display(Name = "Yêu cầu chỉnh sửa")]
    RequiresRevision = 3
  }
}

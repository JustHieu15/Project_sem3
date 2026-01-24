// ViewModels/EmployeeReportSummary.cs
namespace AspnetCoreMvcStarter.ViewModels
{
  public class EmployeeReportSummary
  {
    public int UserId { get; set; }
    public string UserName { get; set; }
    public int TotalTasks { get; set; }
    public int ApprovedTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int TotalPoints { get; set; }
  }
}

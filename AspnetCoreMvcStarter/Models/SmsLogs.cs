namespace AspnetCoreMvcStarter.Models
{
  public class SmsLogs
  {
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public int InvoicesId { get; set; }
    public Invoices Invoices { get; set; }
    public string CustomerPhoneNumber { get; set; }
    public string Message { get; set; }
    public int Status { get; set; }
    public DateTime SentdAt { get; set; }
  }
}

namespace AspnetCoreMvcStarter.Models
{
  public class Invoices
  {
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public int ServicesId { get; set; }
    public Services Services { get; set; }
    public ICollection<SmsLogs> Smslogs { get; set; }
    public string CustomerPhoneNumber { get; set; }
    public decimal TotalAmount { get; set; }
    public int Status { get; set; }
    public int StatusPayment { get; set; }
    public string BarCode { get; set; }
    public DateTime? PaymentAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<InvoicesItem> InvoicesItems { get; set; }
  }
}

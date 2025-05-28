namespace AspnetCoreMvcStarter.Models
{
  public class User
  {
    public int Id { get; set; }
    public string Role { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string? Password { get; set; }
    public ICollection<Invoices> Invoices { get; set; }
    public ICollection<SmsLogs> SmsLogs { get; set; }
  }
}

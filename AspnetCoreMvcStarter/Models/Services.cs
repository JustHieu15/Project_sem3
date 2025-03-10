namespace AspnetCoreMvcStarter.Models
{
  public class Services
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int Status { get; set; }
    public ICollection<Invoices> Invoices { get; set; }
  }
}

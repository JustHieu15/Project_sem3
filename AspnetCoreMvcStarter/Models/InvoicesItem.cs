namespace AspnetCoreMvcStarter.Models
{
  public class InvoicesItem
  {
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public Invoices Invoice { get; set; }
    public string ItemName { get; set; }
    public int Quantity { get; set; }
    public float Weight { get; set; }

  }
}

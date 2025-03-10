using AspnetCoreMvcStarter.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ZXing;
using ZXing.Common;

namespace AspnetCoreMvcStarter.Extend
{
  public class InvoiceDocument : IDocument
  {
    private readonly Invoices _invoice;

    public InvoiceDocument(Invoices invoice)
    {
      _invoice = invoice;
    }
 
    public DocumentMetadata GetMetadata() => new DocumentMetadata
    {
      Title = $"Invoice_{_invoice.Id}",
      Author = "YourCompany"
    };

    public void Compose(IDocumentContainer container)
    {
      var barcodeImage = GenerateBarcode(_invoice.BarCode);

      container.Page(page =>
      {
        page.Size(PageSizes.A4);
        page.Margin(30);

     
        page.Header()
            .AlignCenter()
            .Text("HÓA ĐƠN DỊCH VỤ")
            .FontSize(24).Bold()
            .Underline()
            .FontColor(Colors.Blue.Medium);

        page.Content().Column(col =>
        {
   
          col.Item().Text($"📌 Khách hàng: {_invoice.CustomerPhoneNumber}")
              .FontSize(14).Bold();
          col.Item().Text($"🔹 Dịch vụ: {_invoice.Services?.Name ?? "N/A"}")
              .FontSize(14);
          col.Item().Text($"💰 Tổng tiền: {_invoice.TotalAmount:N0} VNĐ")
              .FontSize(16).Bold()
              .FontColor(Colors.Red.Medium);

          col.Item()
         .Width(150) 
         .Height(50)
         .Image(barcodeImage);



          col.Item().LineHorizontal(1);

         
          col.Item().Table(table =>
          {
            table.ColumnsDefinition(columns =>
            {
              columns.ConstantColumn(180); 
              columns.RelativeColumn();   
              columns.ConstantColumn(100);
              columns.ConstantColumn(120); 
            });

            table.Header(header =>
            {
              header.Cell().Background(Colors.Grey.Lighten3).Text("🔹 Tên đồ giặt").Bold();
              header.Cell().Background(Colors.Grey.Lighten3).Text("🔢 Số lượng").Bold().AlignCenter();
              header.Cell().Background(Colors.Grey.Lighten3).Text("⚖ Trọng lượng").Bold().AlignCenter();
              header.Cell().Background(Colors.Grey.Lighten3).Text("💲 Thành tiền").Bold().AlignRight();
            });


            foreach (var item in _invoice.InvoicesItems)
            {
              table.Cell().Text(item.ItemName);
              table.Cell().Text(item.Quantity.ToString()).AlignCenter();
              table.Cell().Text(item.Weight.ToString("0.0")).AlignCenter();
              table.Cell().Text((item.Quantity * _invoice.Services.Price).ToString("N0"))
                  .AlignRight();
            }
          });

          col.Item().LineHorizontal(1);


          col.Item().AlignCenter()
              .Text("✨ Cảm ơn quý khách đã sử dụng dịch vụ! ✨")
              .Italic()
              .FontSize(14)
              .FontColor(Colors.Green.Medium);
        });
      });
    }



    private static byte[] GenerateBarcode(string barcodeText)
    {
      var writer = new BarcodeWriterPixelData
      {
        Format = BarcodeFormat.CODE_128,
        Options = new EncodingOptions
        {
          Height = 100,
          Width = 300,
          Margin = 10
        }
      };

      var pixelData = writer.Write(barcodeText);
      using (var bitmap = new System.Drawing.Bitmap(pixelData.Width, pixelData.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
      {
        var data = bitmap.LockBits(
            new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
            System.Drawing.Imaging.ImageLockMode.WriteOnly,
            bitmap.PixelFormat);

        try
        {
          System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, data.Scan0, pixelData.Pixels.Length);
        }
        finally
        {
          bitmap.UnlockBits(data);
        }

        using (MemoryStream ms = new MemoryStream())
        {
          bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
          return ms.ToArray();
        }
      }
    }

     

   







  }
}

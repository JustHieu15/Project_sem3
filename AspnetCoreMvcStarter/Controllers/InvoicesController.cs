using AspnetCoreMvcStarter.Areas.Admin.Context;
using AspnetCoreMvcStarter.Extend;
using AspnetCoreMvcStarter.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using ZXing;
using ZXing.Common;


namespace AspnetCoreMvcStarter.Controllers
{
  public class InvoicesController : Controller
  {
    private readonly ApplicationDbContext _context;

    public InvoicesController(ApplicationDbContext context)
    {
      _context = context;
    }

    #region

    public async Task<IActionResult> Index(string searchPhone, int? status, int? statusPayment, int page = 1)
    {
      int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
      if (userId == 0)
        return RedirectToAction("Login", "Auth");

      int pageSize = 10;
      var query = _context.Invoices
          .Include(i => i.InvoicesItems)
          .Where(i => i.UserId == userId)
          .AsQueryable();

      if (!string.IsNullOrEmpty(searchPhone))
      {
        query = query.Where(i => i.CustomerPhoneNumber.Contains(searchPhone));
      }

      if (status.HasValue)
      {
        query = query.Where(i => i.Status == status.Value);
      }

      if (statusPayment.HasValue)
      {
        query = query.Where(i => i.StatusPayment == statusPayment.Value);
      }

      int totalItems = await query.CountAsync();

      var invoices = await query
          .OrderByDescending(i => i.CreatedAt)
          .Skip((page - 1) * pageSize)
          .Take(pageSize)
          .ToListAsync();

      ViewBag.SearchPhone = searchPhone;
      ViewBag.Status = status;
      ViewBag.StatusPayment = statusPayment;
      ViewBag.CurrentPage = page;
      ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

      return View(invoices);
    }


    [HttpPost]
    public async Task<IActionResult> PayInvoice(int id)
    {
      var invoice = await _context.Invoices.FindAsync(id);
      if (invoice == null) return NotFound();

      invoice.StatusPayment = 1;
      invoice.PaymentAt = DateTime.Now;
      invoice.UpdatedAt = DateTime.Now;

      await _context.SaveChangesAsync();
      return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int id, int status)
    {
      var invoice = await _context.Invoices.FindAsync(id);
      if (invoice == null) return NotFound();

      if (status < 0 || status > 2) return BadRequest("Trạng thái không hợp lệ");

      invoice.Status = status;
      invoice.UpdatedAt = DateTime.Now;

      await _context.SaveChangesAsync();
      if (status == 2)
      {
        if (invoice == null)
          return NotFound();
        string accountSid = "";
        string authToken = "";
        string fromPhoneNumber = "";

        // TwilioClient.Init(accountSid, authToken);

        var message = await MessageResource.CreateAsync(
            body: $"Xin chào, đơn hàng #{invoice.Id} của bạn đã hoàn thành! Tổng tiền: {invoice.TotalAmount:C}.",
            from: new PhoneNumber(fromPhoneNumber),
            to: new PhoneNumber("+18777804236")
        );
        var smsLog = new SmsLogs
        {
          UserId = invoice.UserId,
          InvoicesId = invoice.Id,
          CustomerPhoneNumber = invoice.CustomerPhoneNumber,
          Message = message.Body,
          Status = 1,
          SentdAt = DateTime.Now
        };

        _context.SmsLogs.Add(smsLog);
        await _context.SaveChangesAsync();
      }
      return RedirectToAction("Index");
    }
    public IActionResult GenerateBarcode(string barcode)
    {
      if (string.IsNullOrEmpty(barcode))
      {
        return BadRequest("Mã barcode không hợp lệ");
      }

      var writer = new BarcodeWriterPixelData
      {
        Format = BarcodeFormat.CODE_128,
        Options = new EncodingOptions
        {
          Width = 300,
          Height = 100,
          Margin = 10
        }
      };

      var pixelData = writer.Write(barcode);
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
          return File(ms.ToArray(), "image/png");
        }
      }
    }


    [HttpGet]
    public JsonResult GetCustomerByPhone(string phoneNumber)
    {
      var customer = _context.Users
          .Where(u => u.Phone == phoneNumber)
          .Select(u => new { u.Name, u.Address })
          .FirstOrDefault();

      if (customer == null)
      {
        return Json(new { success = false, message = "Không tìm thấy khách hàng" });
      }

      return Json(new { success = true, name = customer.Name, address = customer.Address });
    }

    public IActionResult Create()
    {
      ViewBag.Services = _context.Services.Where(s => s.Status == 1).ToList();
      return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Invoices invoice)
    {
      invoice.UserId = HttpContext.Session.GetInt32("UserId") ?? 0;
      invoice.CreatedAt = DateTime.Now;
      invoice.UpdatedAt = DateTime.Now;
      invoice.Status = 1;
      invoice.StatusPayment = 0;
      invoice.PaymentAt = null;
      invoice.InvoicesItems = new List<InvoicesItem>();
      invoice.Smslogs = new List<SmsLogs>();
      invoice.TotalAmount = 0;
      invoice.CustomerPhoneNumber = invoice.CustomerPhoneNumber;
      invoice.BarCode = invoice.BarCode;
      invoice.ServicesId = invoice.ServicesId;
      invoice.CreatedAt = DateTime.Now;
      invoice.UpdatedAt = DateTime.Now;
      _context.Invoices.Add(invoice);
      _context.SaveChanges();
      return RedirectToAction("Details", new { id = invoice.Id });
    }

    public IActionResult Details(int id)
    {
      var invoice = _context.Invoices
          .Include(i => i.Services) // Bao gồm bảng Services để lấy tên dịch vụ
          .Include(i => i.InvoicesItems) // Bao gồm InvoiceItems
          .FirstOrDefault(i => i.Id == id);

      if (invoice == null)
      {
        return NotFound();
      }

      return View(invoice);
    }


    [HttpPost]
    public IActionResult AddInvoiceItem(int invoiceId, string itemName, int quantity, float weight)
    {
      var invoice = _context.Invoices.Find(invoiceId);
      if (invoice == null)
      {
        return NotFound();
      }

      var service = _context.Services.Find(invoice.ServicesId);
      if (service == null)
      {
        return BadRequest("Dịch vụ không hợp lệ");
      }

      var newItem = new InvoicesItem
      {
        InvoiceId = invoiceId,
        ItemName = itemName,
        Quantity = quantity,
        Weight = weight
      };

      _context.InvoicesItems.Add(newItem);
      _context.SaveChanges();

      // Cập nhật tổng tiền hóa đơn
      invoice.TotalAmount = _context.InvoicesItems
          .Where(i => i.InvoiceId == invoice.Id)
          .Sum(i => i.Quantity * service.Price);
      _context.SaveChanges();

      return RedirectToAction("Details", new { id = invoiceId });
    }

    [HttpPost]
    public IActionResult EditInvoiceItem(int itemId, int quantity, float weight)
    {
      var item = _context.InvoicesItems.Find(itemId);
      if (item == null)
      {
        return NotFound();
      }

      item.Quantity = quantity;
      item.Weight = weight;
      _context.SaveChanges();

      // Cập nhật lại tổng tiền hóa đơn
      var invoice = _context.Invoices.Find(item.InvoiceId);
      var service = _context.Services.Find(invoice.ServicesId);
      invoice.TotalAmount = _context.InvoicesItems
          .Where(i => i.InvoiceId == invoice.Id)
          .Sum(i => i.Quantity * service.Price);
      _context.SaveChanges();

      return RedirectToAction("Details", new { id = item.InvoiceId });
    }

    [HttpPost]
    public IActionResult DeleteInvoiceItem(int itemId)
    {
      var item = _context.InvoicesItems.Find(itemId);
      if (item == null)
      {
        return NotFound();
      }

      var invoiceId = item.InvoiceId;
      _context.InvoicesItems.Remove(item);
      _context.SaveChanges();

      // Cập nhật lại tổng tiền hóa đơn
      var invoice = _context.Invoices.Find(invoiceId);
      var service = _context.Services.Find(invoice.ServicesId);
      invoice.TotalAmount = _context.InvoicesItems
          .Where(i => i.InvoiceId == invoice.Id)
          .Sum(i => i.Quantity * service.Price);
      _context.SaveChanges();

      return RedirectToAction("Details", new { id = invoiceId });
    }


    #endregion





    public IActionResult PrintInvoice(int id)
    {
      var invoice = _context.Invoices
          .Include(i => i.Services)
          .Include(i => i.InvoicesItems)
          .FirstOrDefault(i => i.Id == id);

      if (invoice == null) return NotFound();
      if (invoice == null)
      {
        throw new Exception("Không tìm thấy hóa đơn!");
      }

      if (invoice.Services == null)
      {
        throw new Exception("Hóa đơn không có dịch vụ!");
      }
      var pdfDocument = new InvoiceDocument(invoice);
      var pdfBytes = pdfDocument.GeneratePdf();

      return File(pdfBytes, "application/pdf", $"Invoice_{id}.pdf");
    }

























  }
}

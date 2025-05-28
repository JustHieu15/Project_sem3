// using AspnetCoreMvcStarter.Areas.Admin.Context;
// using AspnetCoreMvcStarter.Models;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using Twilio;
// using Twilio.Rest.Api.V2010.Account;
// using Twilio.Types;
//
// namespace AspnetCoreMvcStarter.Controllers
// {
//   public class SmsController : Controller
//   {
//     private readonly ApplicationDbContext _context;
//
//     public SmsController(ApplicationDbContext context)
//     {
//       _context = context;
//     }
//
//     public async Task<IActionResult> Index()
//     {
//       var invoices = await _context.Invoices
//         .Where(i => i.Status == 2 && !i.Smslogs.Any(s => s.Status == 1))
//         .Include(i => i.Smslogs)
//         .ToListAsync();
//
//       return View(invoices);
//     }
//
//     [HttpPost]
//     public IActionResult ClearTempData()
//     {
//       TempData.Clear();
//       return Ok();
//     }
//
//     [HttpPost]
//     public async Task<IActionResult> SendSms(int id)
//     {
//       var invoice = await _context.Invoices.FindAsync(id);
//       if (invoice == null)
//         return NotFound();
//       // string accountSid = "";
//       // string authToken = "";
//       // string fromPhoneNumber = "";
//
//       // TwilioClient.Init(accountSid, authToken);
//
//         var message = await MessageResource.CreateAsync(
//             body: $"Xin chào, đơn hàng #{invoice.Id} của bạn đã hoàn thành! Tổng tiền: {invoice.TotalAmount:C}.",
//             from: new PhoneNumber(fromPhoneNumber),
//             to: new PhoneNumber("")
//         );
//         var smsLog = new SmsLogs
//         {
//           UserId = invoice.UserId,
//           InvoicesId = invoice.Id,
//           CustomerPhoneNumber = invoice.CustomerPhoneNumber,
//           Message = message.Body,
//           Status = 1,
//           SentdAt = DateTime.Now
//         };
//
//         _context.SmsLogs.Add(smsLog);
//         await _context.SaveChangesAsync();
//
//       return RedirectToAction("Index", new { message = "Tin nhắn đã được gửi thành công!" });
//     }
//
//     public async Task<IActionResult> SentInvoices()
//     {
//       var invoices = await _context.Invoices
//           .Where(i => i.Smslogs.Any(s => s.Status == 1)) // Chỉ lấy hóa đơn có SMS đã gửi
//           .Include(i => i.Smslogs)
//           .ToListAsync();
//
//       return View(invoices);
//     }
//
//
//
//   }
// }
//
//
//

using ERP.Data;
using ERP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;

namespace ERP.Controllers
{
    [Authorize]
    public class MailController(ERPDbContext context, IConfiguration config) : Controller
    {
        #region Inbox Views
        public IActionResult Inbox()
        {
            int userId = int.Parse(User.FindFirst("UserId")!.Value);
            return View(context.Mails.Where(m => m.ReceiverId == userId).ToList());
        }

        public IActionResult Send()
        {
            return View();
        }

        #endregion

        #region Mail Credintional & Send 

        [HttpPost]
        public IActionResult Send(Mail mail)
        {
            int senderId = int.Parse(User.FindFirst("UserId")!.Value);
            mail.SenderId = senderId;
            mail.SentDate = DateTime.Now;

            try
            {
                SendRealEmail(mail); // ✅ FIRST send email
                context.Mails.Add(mail); // ✅ THEN save
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Email sending failed: " + ex.Message);
                return View(mail);
            }

            return RedirectToAction(nameof(Inbox));
        }

        private void SendRealEmail(Mail mail)
        {
            var receiver = context.Users.Find(mail.ReceiverId);
            if (receiver == null || string.IsNullOrEmpty(receiver.Email))
                throw new Exception("Receiver email not found");

            var smtp = new SmtpClient
            {
                Host = config["Smtp:Host"]!,
                Port = int.Parse(config["Smtp:Port"]!),
                EnableSsl = true,
                Credentials = new NetworkCredential(
                    config["Smtp:Email"],
                    config["Smtp:Password"]
                )
            };

            var message = new MailMessage
            {
                From = new MailAddress(config["Smtp:Email"]!, "ERP System"),
                Subject = mail.Subject,
                Body = mail.Body,
                IsBodyHtml = false
            };

            message.To.Add(receiver.Email);

            smtp.Send(message); 
        }

        #endregion
    }
}

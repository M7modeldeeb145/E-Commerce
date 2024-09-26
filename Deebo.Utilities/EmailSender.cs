using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DeeboStore.Utilities
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string ConfirmLink)
        {
            var message = new MailMessage();
            var smtpclient = new SmtpClient();
            message.From = new MailAddress("hassanzidan861@gmail.com");
            message.Subject = subject;
            message.To.Add(email);
            message.Body = ConfirmLink;
            message.IsBodyHtml = true;

            smtpclient.Port = 587;
            smtpclient.Host = "smtp.gmail.com";
            smtpclient.EnableSsl = true;
            smtpclient.Credentials = new NetworkCredential("hassanzidan861@gmail.com", "vpspudpvghuntgqp");
            smtpclient.Send(message);
            return Task.CompletedTask;
        }
    }
}

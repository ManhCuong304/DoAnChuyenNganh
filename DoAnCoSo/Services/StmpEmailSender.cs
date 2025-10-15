using System.Net.Mail;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace DoAnCoSo.Services
{
    public class StmpEmailSender : IEmailSender
    {
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            using var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new System.Net.NetworkCredential("letoanban.word@gmail.com", "meob qhxj axnf fqtx"),
                EnableSsl = true
            };
            var mailMessage = new MailMessage
            {
                From = new MailAddress("letoanban.word@gmail.com"),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };
            mailMessage.To.Add(email);
            await client.SendMailAsync(mailMessage);
        }
    }
}

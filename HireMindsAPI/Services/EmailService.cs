using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace HireMindsAPI.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            var host = _config["Smtp:Host"];
            var port = int.Parse(_config["Smtp:Port"] ?? "587");
            var username = _config["Smtp:Username"];
            var password = _config["Smtp:AppPassword"];

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || username == "your_email@gmail.com")
            {
                // If not configured, just log to console (useful for development without sending actual emails)
                Console.WriteLine($"[EMAIL MOCK] To: {toEmail} | Subject: {subject} | Body: {htmlMessage}");
                return;
            }

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(username!, "HireMinds"),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
        }
    }
}

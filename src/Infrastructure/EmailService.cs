using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Settings;
using Microsoft.Extensions.Options;

namespace CleanArchitectureBase.Infrastructure
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var smtpClient = new SmtpClient(_settings.SmtpHost)
            {
                Port = _settings.SmtpPort,
                Credentials = new NetworkCredential(_settings.SmtpUser, _settings.SmtpPass),
                EnableSsl = _settings.EnableSsl,
            };

            var mailMessage = new MailMessage(_settings.From, to, subject, body)
            {
                IsBodyHtml = true
            };

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}

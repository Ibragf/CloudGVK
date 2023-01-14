using BackendGVK.Services.Configs;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace BackendGVK.Services.EmailSender
{
    public class EmailSender : IEmailSender
    {
        private readonly SmtpServerSettings _smtpSettings;
        public EmailSender(IOptions<SmtpServerSettings> smtpSettings) {
            _smtpSettings = smtpSettings.Value;
        }
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("CloudGVK", _smtpSettings.User));
            emailMessage.To.Add(new MailboxAddress(email, email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = message
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_smtpSettings.ServerSettings.Host, _smtpSettings.ServerSettings.Port, _smtpSettings.ServerSettings.UseSsl);
                await client.AuthenticateAsync(_smtpSettings.User, _smtpSettings.Password);
                await client.SendAsync(emailMessage);

                await client.DisconnectAsync(true);
            }
        }
    }
}

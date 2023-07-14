using BackendGVK.Services.Configs;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Text;

namespace BackendGVK.Services.EmailSender
{
    public class EmailSender : IEmailSender
    {
        private readonly SmtpServerSettings _smtpSettings;
        private readonly string html;
        public EmailSender(IOptions<SmtpServerSettings> smtpSettings) {
            _smtpSettings = smtpSettings.Value;
            byte[] buffer;
            using (var fs = new FileStream("mail.txt", FileMode.Open))
            {
                buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
            }
            html = Encoding.UTF8.GetString(buffer);
        }

        public string GetHtmlForConfirmationToken(string token)
        {
            return html.Replace("23423423", token);
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

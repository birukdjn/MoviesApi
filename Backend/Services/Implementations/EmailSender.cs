using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Backend.Configuration;
using Backend.Models;
using Backend.Services.Interfaces;

namespace Backend.Services.Implementations
{
    public class EmailSender(EmailConfiguration emailConfig):IEmailSender
    {
        private readonly EmailConfiguration _emailConfig = emailConfig;

        public void SendEmail(Message message)
        {
            var emailMessage = CreateEmailMessage(message);

            using var client = new SmtpClient();
            try
            {
                client.Connect(_emailConfig.SmtpServer, _emailConfig.Port, SecureSocketOptions.StartTls);
                client.Authenticate(_emailConfig.UserName, _emailConfig.Password);
                client.Send(emailMessage);
            }
            catch
            {
                // Log the exception here
                throw;
            }
            finally
            {
                client.Disconnect(true);
            }
        }

        private MimeMessage CreateEmailMessage(Message message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Birukdjn", _emailConfig.From));
            emailMessage.To.AddRange(message.To.Select(t => new MailboxAddress("", t)));
            emailMessage.Subject = message.Subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = message.Content };

            return emailMessage;
        }
    }
}



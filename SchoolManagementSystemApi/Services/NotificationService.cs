using MailKit.Net.Smtp;
using MimeKit;
using System.Net.Mail;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace SchoolManagementSystemApi.Services
{
    public class NotificationService
    {
        private readonly IConfiguration _configuration;

        public NotificationService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void SendSms(string to, string message)
        {
            var accountSid = _configuration["Twilio:AccountSid"];
            var authToken = _configuration["Twilio:AuthToken"];
            var fromNumber = _configuration["Twilio:FromNumber"];
            TwilioClient.Init(accountSid, authToken);
            MessageResource.Create(
                body: message,
                from: new Twilio.Types.PhoneNumber(fromNumber),
                to: new Twilio.Types.PhoneNumber(to)
            );
        }

        public void SendEmail(string to, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("School", _configuration["Email:From"]));
            message.To.Add(new MailboxAddress("", to));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using var client = new MailKit.Net.Smtp.SmtpClient();
            client.Connect(_configuration["Email:SmtpHost"], int.Parse(_configuration["Email:SmtpPort"]), true);
            client.Authenticate(_configuration["Email:Username"], _configuration["Email:Password"]);
            client.Send(message);
            client.Disconnect(true);
        }
    }
}
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Net.Mail;

namespace Jwt_Tutorial.api.Services
{
    public interface IEmailService
    {
        Task SendEmail(string reciever, string subject, string body);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmail(string reciever, string subject, string body)
        {
            var email = _config.GetValue<string>("EMAIL_CONFIGURATION:EMAIL");
            var password = _config.GetValue<string>("EMAIL_CONFIGURATION:PASSWORD");
            var server = _config.GetValue<string>("EMAIL_CONFIGURATION:SERVER");
            var port = _config.GetValue<int>("EMAIL_CONFIGURATION:PORT");

            var smtpClient = new SmtpClient(server, port);
            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;

            smtpClient.Credentials = new NetworkCredential(email, password);

            var message = new MailMessage(email!, reciever, subject, body);

            await smtpClient.SendMailAsync(message);
        }
    }
}

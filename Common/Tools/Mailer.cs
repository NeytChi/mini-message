using System;
using System.Net;
using System.Net.Mail;
using mini_message.Common.Settings;
using Serilog;

namespace mini_message.Common.Tools
{
    public class Mailer : IMailer
    {
        private readonly ILogger _log;
        private readonly SmtpSettings _settings;
        private const string Host = "mimicry.messanger";
        private readonly MailAddress _from;
        private readonly SmtpClient _smtp;

        public Mailer(ILogger log, SmtpSettings smtpSettings)
        {
            _log = log;
            _settings = smtpSettings;
            _smtp = new SmtpClient(_settings.SmtpServer, _settings.SmtpPort) {
                EnableSsl = true,
                Credentials = new NetworkCredential(_settings.MailAddress, _settings.MailPassword)
            };
            _from = new MailAddress(_settings.MailAddress, Host);
            log.Information("Setting up mailer.");
        }
        public async void SendEmail(string email, string subject, string text)
        {
            var to = new MailAddress(email);
            var message = new MailMessage(_from, to)
            {
                Subject = subject, Body = text, IsBodyHtml = true
            };
            try {
                if (_settings.EmailEnable)
                    await _smtp.SendMailAsync(message);
                _log.Information("Send message to " + email);
            }
            catch (Exception e) {
                _log.Error("Can't send email message, ex: " + e.Message);
            }
        }
    }
}
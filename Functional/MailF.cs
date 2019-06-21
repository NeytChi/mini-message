using System;
using System.Net;
using Common.Logging;
using System.Net.Mail;
using Newtonsoft.Json.Linq;

namespace Common.Functional.Mail
{
    public static class MailF
    {
        private static string GmailServer = "smtp.gmail.com";
        private static int GmailPort = 587;
        private static string ip = "127.0.0.1";
        private static string domen = "minimessanger";
        private static string mailAddress;
        private static string mailPassword;
        private static MailAddress from;
        private static SmtpClient smtp;

        public static void Init()
        {
            ip = Config.IP;
            domen = Config.Domen;
            mailAddress = Config.GetConfigValue("mail_address", JTokenType.String);
            mailPassword = Config.GetConfigValue("mail_password", JTokenType.String);
            GmailServer = Config.GetConfigValue("smtp_server", JTokenType.String);
            GmailPort = Config.GetConfigValue("smtp_port", JTokenType.Integer);
            if (ip != null && mailAddress != null)
            {
                smtp = new SmtpClient(GmailServer, GmailPort);
                smtp.Credentials = new NetworkCredential(mailAddress, mailPassword);
                from = new MailAddress(mailAddress, domen);
                smtp.EnableSsl = true;
            }
        }
        /// <summary>
        /// Sends the email.
        /// </summary>
        /// <param name="emailAddress">Email address.</param>
        /// <param name="subject">Subject.</param>
        /// <param name="message">Message.</param>
        public static async void SendEmail(string emailAddress, string subject, string text)
        {
            MailAddress to = new MailAddress(emailAddress);
            MailMessage message = new MailMessage(from, to);
            message.Subject = subject;
            message.Body = text;
            message.IsBodyHtml = true;
            try
            {
                await smtp.SendMailAsync(message);
                Logger.WriteLog("Send message to " + emailAddress, LogLevel.Usual);
            }
            catch (Exception e)
            {
                Logger.WriteLog("Error SendEmailAsync, Message:" + e.Message, LogLevel.Error);
                smtp = new SmtpClient(GmailServer, GmailPort);
                smtp.Credentials = new NetworkCredential(mailAddress, mailPassword);
                smtp.EnableSsl = true;
                await smtp.SendMailAsync(message);
                Logger.WriteLog("Send message to " + emailAddress, LogLevel.Usual);
            }
        }
    }
}
/*
using System;
using MimeKit;
using Common.Logging;
using MailKit.Security;
using MailKit.Net.Smtp;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Common.Functional.Mail
{
    public static class MailF
    {
        private static MailboxAddress hostMail;
        private static string domen = "127.0.0.1";
        private static string mailAddress;
        private static string mailPassword;
        private static string GmailServer = "smtp.gmail.com";
        private static int GmailPort = 587;
        private static SmtpClient client = new SmtpClient();

        public static void Init()
        {
            domen = Config.GetConfigValue("domen", "string");
            mailAddress = Config.GetConfigValue("mail_address", "string");
            mailPassword = Config.GetConfigValue("mail_password", "string");
            GmailServer = Config.GetConfigValue("smtp_server", "string");
            GmailPort = Config.GetConfigValue("smtp_port", "int");
            if (domen != null && mailAddress != null)
            {
                Logger.WriteLog("Connection to smpt ip=" + domen + ";address=" + mailAddress + ";password=" + mailPassword + ";server=" + GmailServer + ";port=" + GmailPort + ";", LogLevel.Usual);
                hostMail = new MailboxAddress(domen, mailAddress);
                //client.ServerCertificateValidationCallback = (object sender,
                //X509Certificate certificate,
                //X509Chain chain,
                //SslPolicyErrors sslPolicyErrors) => true;
                //client.SslProtocols = System.Security.Authentication.SslProtocols.Default;
                //client.CheckCertificateRevocation = false;
                //client.SslProtocols = System.Security.Authentication.SslProtocols.Tls11;
                client.Connect(GmailServer, GmailPort, SecureSocketOptions.StartTls);
                client.Authenticate(mailAddress, mailPassword);
                client.NoOp();
                Logger.WriteLog("Connect to mail-server:" + GmailServer + " is successfully.", LogLevel.Usual);
            }
        }
        /// <summary>
        /// Sends the email.
        /// </summary>
        /// <param name="emailAddress">Email address.</param>
        /// <param name="subject">Subject.</param>
        /// <param name="message">Message.</param>
        public static void SendEmail(string emailAddress, string subject, string message)
        {
            MimeMessage emailMessage = new MimeMessage();
            emailMessage.From.Add(hostMail);
            emailMessage.To.Add(new MailboxAddress(emailAddress));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = message
            };
            try
            {
                if (!client.IsConnected && !client.IsAuthenticated)
                {
                    //client.Connect(GmailServer, GmailPort);
                    //client.Authenticate(mailAddress, mailPassword);
                }
                //client.Send(emailMessage);
                Logger.WriteLog("Send message to " + emailAddress, LogLevel.Usual);
            }
            catch (Exception e)
            {
                Logger.WriteLog("Error SendEmail(), Message:" + e.Message, LogLevel.Error);
            }
        }
    }
}










*/

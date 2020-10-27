namespace mini_message.Common.Settings
{
    public class SmtpSettings
    {
        public string MailAddress { get; set; }
        public string MailPassword { get; set; }
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public bool EmailEnable { get; set; }
    }
}
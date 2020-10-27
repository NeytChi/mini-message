namespace mini_message.Common.Tools
{
    public interface IMailer
    {
        void SendEmail(string email, string subject, string text);
    }
}
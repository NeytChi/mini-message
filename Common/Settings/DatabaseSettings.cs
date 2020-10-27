namespace mini_message.Common.Settings
{
    public class DatabaseSettings
    {
        public string Server { get; set; } 
        public string Database { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Pooling { get; set; }
        public string SslMode { get; set; }
    }
}
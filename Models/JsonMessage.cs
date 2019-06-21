namespace Common
{
    public class JsonMessage
    {
        public bool success;
        public string message;
        public JsonMessage(bool success, string message)
        {
            this.success = success;
            this.message = message;
        }
    }
}
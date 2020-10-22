namespace mini_message.Dtos
{
    public class MessageResponse
    {
        public bool success;
        public string message;
        public MessageResponse(bool success, string message)
        {
            this.success = success;
            this.message = message;
        }
    }
}
namespace mini_message.Dtos
{
    public class MessageResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
        public MessageResponse(bool success, string message)
        {
            this.success = success;
            this.message = message;
        }
    }
}
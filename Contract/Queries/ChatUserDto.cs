namespace mini_message.Contract.Queries
{
    public class ChatUserDto
    {
        public int chat_id { get; set; }
        public int user_id { get; set; }
        public bool enable  { get; set; }

        public ChatUserDto()
        {
            enable = false;
        }
    }
}
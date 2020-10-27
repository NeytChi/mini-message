namespace mini_message.Contract.Queries
{
    public class ChatDto
    {
        public UserDto user { get; set; }
        public ChatRoomDto chat { get; set; }
        public MessageDto last_message { get; set; }
    }
}
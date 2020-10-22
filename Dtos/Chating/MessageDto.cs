using System;

namespace mini_message.Dtos.Chating
{
    public struct MessageDto
    {
        public long message_id;
        public long chat_id;
        public int user_id;
        public string message_text;
        public bool message_viewed;
        public DateTime created_at;
    }
}

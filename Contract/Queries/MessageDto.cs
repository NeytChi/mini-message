using System;
using mini_message.Models;

namespace mini_message.Contract.Queries
{
    public class MessageDto
    {

        public long message_id { get; set; }
        public long chat_id { get; set; }
        public int user_id { get; set; }
        public string message_text { get; set; }
        public bool message_viewed { get; set; }
        public DateTimeOffset created_at { get; set; }

        public MessageDto(Message message)
        {
            message_id = message.Id;
            chat_id = message.ChatId;
            user_id = message.UserId;
            message_text = message.Text;
            message_viewed = message.Viewed;
            created_at = message.CreatedAt;
        }
    }
}
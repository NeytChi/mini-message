using System;
using System.Collections.Generic;
using mini_message.Models;

namespace mini_message.Contract.Queries
{
    public class ChatRoomDto
    {
        public long chat_id { get; set; }
        public string chat_token { get; set; }
        public DateTimeOffset created_at { get; set; }
        public List<ChatUserDto> users { get; set; }

        public ChatRoomDto(ChatRoom room)
        {
            chat_id = room.Id;
            chat_token = room.Token;
            created_at = room.CreatedAt;
        }
    }
}
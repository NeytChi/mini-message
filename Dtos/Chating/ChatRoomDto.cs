using System;
using System.Collections.Generic;
using Common.Chats;

namespace mini_message.Dtos.Chating
{
    public struct ChatRoomDto
    {
        public int chat_id;
        public string chat_token;
        public DateTime created_at;
        public List<ChatUser> users;
    }
}

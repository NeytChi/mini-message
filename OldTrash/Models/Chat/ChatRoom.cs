using System;
using System.Collections.Generic;

namespace Common.Chats
{
    public struct ChatRoom
    {
        public int chat_id;
        public string chat_token;
        public DateTime created_at;
        public List<ChatUser> users;
    }
}

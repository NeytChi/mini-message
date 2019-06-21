using System;
using System.IO;
using System.Collections.Generic;

namespace Common.Chats
{
    public class ChatRoom
    {
        public int chat_id;
        public int creator_id;
        public int message_count = 0;
        public DateTime created_at = DateTime.Now;
        public List<ChatUser> users = new List<ChatUser>();
    }
}

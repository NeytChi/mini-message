﻿using System;
namespace Common.Chats
{
    public class Message
    {
        public long message_id;
        public long chat_id;
        public int user_id;
        public string user_login;
        public string message_text;
        public bool message_viewed;
        public DateTime created_at = DateTime.Now;
    }
}

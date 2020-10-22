using System;
using Common.Chats;
using Common.NDatabase.UserData;

namespace MiniMessanger.Models.Chat
{
    public struct ChatData
    {
        public dynamic user;
        public ChatRoom chat;
        public Message last_message;
    }
}

﻿using System;
using Common.Chats;
using Common.NDatabase.UserData;

namespace MiniMessanger.Models.Chat
{
    public struct ChatData
    {
        public UserCache user;
        public ChatRoom chat;
        public Message last_message;
    }
}
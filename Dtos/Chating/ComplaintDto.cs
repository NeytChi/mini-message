﻿using System;

namespace mini_message.Dtos.Chating
{
    public struct ComplaintDto
    {
        public int complaint_id;
        public int user_id;
        public int blocked_id;
        public long message_id;
        public string complaint;
        public DateTime created_at;
    }
}

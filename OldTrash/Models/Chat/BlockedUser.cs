using System;
namespace MiniMessanger.Models.Chat
{
    public struct BlockedUser
    {
        public int blocked_id;
        public int user_id;
        public int blocked_user_id;
        public string blocked_reason;
        public bool blocked_deleted;
    }
}

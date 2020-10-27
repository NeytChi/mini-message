using System.Collections.Generic;

namespace mini_message.Contract.Commands
{
    public class GetNonBlockedUsersCommand
    {
        public int Id { get; set; }
        public int Since { get; set; }
        public int Count { get; set; }
        public ICollection<int> BlockedUsers { get; set; }
    }
}
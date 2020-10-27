using mini_message.Models;

namespace mini_message.Contract.Queries
{
    public class BlockedUserDto
    {
        public string user_email { get; set; }
        public string user_login { get; set; }
        public long last_login_at { get; set; }
        public string user_public_token { get; set; }
        public string blocked_reason { get; set; }

        public BlockedUserDto(BlockedUser blockedUser, User user)
        {
            user_email = user.Email;
            user_login = user.Login;
            last_login_at = user.LastLoginAt;
            user_public_token = user.PublicToken;
            blocked_reason = blockedUser.Reason;
        }
    }
}
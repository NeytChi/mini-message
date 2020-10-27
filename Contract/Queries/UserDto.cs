using mini_message.Models;

namespace mini_message.Contract.Queries
{
    public class UserDto
    {
        public int user_id { get; set; }
        public string user_email { get; set; }
        public string user_login { get; set; }
        public long created_at { get; set; }
        public long last_login_at { get; set; }
        public string user_public_token { get; set; }
        
        public UserDto(User user)
        {
            user_id = user.Id;
            user_email = user.Email;
            user_login = user.Login;
            created_at = user.CreatedAt;
            last_login_at = user.LastLoginAt;
            user_public_token = user.PublicToken;
        }
    }
}
using System.ComponentModel.DataAnnotations;

namespace mini_message.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Email { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Hash { get; set; }
        public short Activate { get; set; }
        public long CreatedAt { get; set; }
        public string Token { get; set; }
        public long LastLoginAt { get; set; }
        public int RecoveryCode { get; set; }
        public string RecoveryToken { get; set; }
        public string PublicToken { get; set; }
        public Profile Profile { get; set; }
    }
}
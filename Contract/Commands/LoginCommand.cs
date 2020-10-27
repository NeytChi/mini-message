using System.ComponentModel.DataAnnotations;

namespace mini_message.Contract.Commands
{
    public class LoginCommand
    {
        [Required]
        public string user_email { get; set; }
        [Required]
        public string user_password { get; set; }
    }
}
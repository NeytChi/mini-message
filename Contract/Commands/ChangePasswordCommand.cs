using System.ComponentModel.DataAnnotations;

namespace mini_message.Contract.Commands
{
    public class ChangePasswordCommand
    {
        [Required]
        public string recovery_token { get; set; }
        [Required]
        public string user_password { get; set; }
        [Required]
        public string user_confirm_password { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace mini_message.Contract.Commands
{
    public class CheckRecoveryCodeCommand
    {
        [Required]
        public int recovery_code { get; set; }
        [Required]
        public string user_email { get; set; }
    }
}
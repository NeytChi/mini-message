using System.ComponentModel.DataAnnotations;

namespace mini_message.Contract.Commands
{
    public class RecoveryPasswordCommand
    {
        [Required]
        public string user_email { get; set; }
    }
}
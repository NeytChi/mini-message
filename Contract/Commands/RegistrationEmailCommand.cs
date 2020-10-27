using System.ComponentModel.DataAnnotations;

namespace mini_message.Contract.Commands
{
    public class RegistrationEmailCommand
    {
        [Required]
        public string user_email { get; set; }
    }
}
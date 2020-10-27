using System.ComponentModel.DataAnnotations;

namespace mini_message.Contract.Commands
{
    public class RegistrationCommand
    {
        [Required]
        public string user_email { get; set; }
        [Required]
        public string user_login { get; set; }
        [Required]
        public string user_password { get; set; }
    }
}
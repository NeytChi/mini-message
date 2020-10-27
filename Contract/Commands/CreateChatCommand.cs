using System.ComponentModel.DataAnnotations;

namespace mini_message.Contract.Commands
{
    public class CreateChatCommand
    {
        [Required]
        public string user_token { get; set; }
        [Required]
        public string opposide_public_token { get; set; }
    }
}
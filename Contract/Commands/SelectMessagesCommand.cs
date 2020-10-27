using System.ComponentModel.DataAnnotations;

namespace mini_message.Contract.Commands
{
    public class SelectMessagesCommand
    {
        [Required]
        public string user_token { get; set; }
        [Required]
        public string chat_token { get; set; }
    }
}
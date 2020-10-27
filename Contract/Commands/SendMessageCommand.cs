using System.ComponentModel.DataAnnotations;

namespace mini_message.Contract.Commands
{
    public class SendMessageCommand
    {
        [Required]
        public string user_token { get; set; }
        [Required]
        public string chat_token { get; set; }
        [Required]
        public string message_text { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace mini_message.Contract.Commands
{
    public class DeleteCommand
    {
        [Required]
        public string user_token { get; set; }
    }
}
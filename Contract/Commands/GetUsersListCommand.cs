using System.ComponentModel.DataAnnotations;

namespace mini_message.Contract.Commands
{
    public class GetUsersListCommand
    {
        [Required]
        public string user_token { get; set; }
    }
}
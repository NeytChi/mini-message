using System.ComponentModel.DataAnnotations;

namespace mini_message.Contract.Commands
{
    public class ComplaintContentCommand
    {
        [Required]
        public string user_token { get; set; }
        [Required]
        public long message_id { get; set; }
        [Required]
        public string complaint { get; set; }
    }
}
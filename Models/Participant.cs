using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mini_message.Models
{
    public class Participant
    {
        [Key]
        public long Id { get; set; }
        [ForeignKey("ChatRoom")]
        public long ChatId { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        [ForeignKey("OpposideUser")]
        public int OpposideId { get; set; }
        public virtual ChatRoom ChatRoom { get; set; }
        public virtual User User { get; set; }
        public virtual User OpposideUser { get; set; }

    }
}
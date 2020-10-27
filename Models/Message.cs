using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mini_message.Models
{
    public class Message
    {
        [Key]
        public long Id { get; set; }
        [ForeignKey("Room")]
        public long ChatId { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        [Column(TypeName = "varchar(500) CHARACTER SET utf8 COLLATE utf8_general_ci")]
        public string Text { get; set; }
        public bool Viewed { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public virtual User User { get; set; }
        public virtual ChatRoom Room { get; set; }
    }
}
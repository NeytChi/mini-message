using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mini_message.Models
{
    public class Complaint
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        [ForeignKey("BlockedUser")]
        public int BlockedId { get; set; }
        [ForeignKey("Message")]
        public long MessageId { get; set; }
        [Column(TypeName = "varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci")]
        public string ComplaintMessage { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public virtual User User { get; set; }
        public virtual BlockedUser BlockedUser { get; set; }
        public virtual Message Message { get; set; }
    }
}
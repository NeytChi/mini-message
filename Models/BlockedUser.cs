using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mini_message.Models
{
    public class BlockedUser
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        [ForeignKey("BlockUser")]
        public int BlockedUserId { get; set; }
        [Column(TypeName = "varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci")]
        public string Reason { get; set; }
        public bool Deleted { get; set; }
        public virtual User User { get; set; }
        public virtual User BlockUser { get; set; }
    }
}
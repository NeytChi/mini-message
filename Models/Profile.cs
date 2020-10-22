using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mini_message.Models
{
    public class Profile
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        public string UrlPhoto { get; set; }
        public User User { get; set; }
    }
}
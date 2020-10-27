using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace mini_message.Models
{
    public class ChatRoom
    {
        [Key]
        public long Id { get; set; }
        public string Token { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
    }
}
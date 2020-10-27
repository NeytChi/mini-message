using System.ComponentModel.DataAnnotations;

namespace mini_message.Models
{
    public class UploadFile
    {
        [Key]
        public int Id { get; set; }
        public string RelativePath { get; set; }
        public string Name { get; set; }
    }
}
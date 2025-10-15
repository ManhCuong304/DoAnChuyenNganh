using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnCoSo.Models
{
    public class Message
    {
        public int Id { get; set; }

        [Required]
        public string FromUserId { get; set; }

        [Required]
        public string ToUserId { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        [ForeignKey("FromUserId")]
        public virtual ApplicationUser FromUser { get; set; }

        [ForeignKey("ToUserId")]
        public virtual ApplicationUser ToUser { get; set; }
    }
}

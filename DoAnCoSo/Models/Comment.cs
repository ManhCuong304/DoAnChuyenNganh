using System;
using System.ComponentModel.DataAnnotations;

namespace DoAnCoSo.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int PostId { get; set; }
        public Post Post { get; set; }

        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
        public int? ParentCommentId { get; set; }
    }
}

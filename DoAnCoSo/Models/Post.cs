using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DoAnCoSo.Models
{
    public class Post
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nội dung không được bỏ trống")]
        public string Content { get; set; }

        public string? ImagePath { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? LikeType { get; set; } 

        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public List<Comment>? Comments { get; set; }

        public string? EmbeddingVector { get; set; }
        public int LikeCount { get; set; }
    }
}

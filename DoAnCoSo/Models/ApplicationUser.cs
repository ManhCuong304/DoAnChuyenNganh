using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DoAnCoSo.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string? Image { get; set; }
        public string? Description { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime? BirthDate { get; set; }
        public string? Major { get; set; }

        public string? Describe { get; set; }
        public string AvatarDisplay
        {
            get
            {
                return string.IsNullOrEmpty(Image)
                    ? "/images/default-avatar.png"   // avatar mặc định
                    : Image;                         // ảnh từ DB
            }
        }

        public List<Follow> Followings { get; set; }
        public List<Follow> Followers { get; set; }

        public ICollection<Post> Posts { get; set; }
        public ICollection<Comment> Comments { get; set; }

        public string? EmbeddingVector { get; set; }

    }
}

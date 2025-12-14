using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace DoAnCoSo.Models
{
    public class ProfileViewModel
    {
        public string? Id { get; set; }
        [Display(Name = "Họ và tên")]
        [Required]
       
        public string FullName { get; set; }
        [Display(Name = "Tên đăng nhập")]
        public string? Email { get; set; }
        public string? UserName { get; set; }
        [Display(Name = "Biệt danh")]
        public string Nickname { get; set; }

        [Display(Name = "Số điện thoại")]
        [Phone]
        public string PhoneNumber { get; set; }

        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "Ngành học")]
        public string? Description { get; set; } 

        [Display(Name = "Chuyên ngành")]
        public string? Major  { get; set; }

        [Display(Name = "Mô tả ")]
        public string? Describe { get; set; }
        public string Image { get; set; }

        [Display(Name = "Ảnh đại diện")]
        public IFormFile Avatar { get; set; }
    }
}

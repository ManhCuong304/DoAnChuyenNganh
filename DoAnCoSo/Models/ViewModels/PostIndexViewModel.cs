using System.Collections.Generic;

namespace DoAnCoSo.Models.ViewModels
{
    public class PostIndexViewModel
    {
        public IEnumerable<Post> Posts { get; set; } = new List<Post>();

        // Dùng để lọc bài viết hoặc tìm kiếm
        public string SearchKeyword { get; set; }
        public string Category { get; set; }
        public string AuthorId { get; set; }

        // Bài viết mới mà người dùng muốn đăng
        public Post NewPost { get; set; } = new Post();

        // Các property mà Controller/View đang dùng
        public HashSet<int> RelevantPostIds { get; set; } = new();
        public string CurrentUserId { get; set; }
        public List<ApplicationUser> MutualFriends { get; set; } = new();
        public HashSet<int> LikedPostIds { get; set; } = new HashSet<int>();
    }
}

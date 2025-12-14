using DoAnCoSo.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
public interface IPostService
{
    Task<List<Post>> GetRecommendedPostsWithAI(ApplicationUser currentUser);
}
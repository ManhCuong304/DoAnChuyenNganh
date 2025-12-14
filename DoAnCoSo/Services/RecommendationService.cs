// Services/RecommendationService.cs
using DoAnCoSo.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Net.Http.Json;

namespace DoAnCoSo.Services
{
    public class RecommendationService
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;

        public RecommendationService(ApplicationDbContext context, HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }

        public async Task<HashSet<int>> GetRecommendedPostIdsAsync(string userId)
        {

            var profile = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new
                {
                    u.Description,  
                    u.Major,       
                    u.Describe      
                })
                .FirstOrDefaultAsync();

            if (profile == null)
                return new HashSet<int>();


            var posts = await _context.Posts
                .Select(p => new
                {
                    p.Id,
                    p.Content
                })
                .ToListAsync();


            var requestPayload = new
            {
                Profile = profile,
                Posts = posts
            };


            var response = await _httpClient.PostAsJsonAsync("/recommend", requestPayload);

            response.EnsureSuccessStatusCode();

            var recommendedIds = await response.Content.ReadFromJsonAsync<List<int>>();

            return recommendedIds?.ToHashSet() ?? new HashSet<int>();
        }
    }
}

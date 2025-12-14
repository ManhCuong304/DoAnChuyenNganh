using DoAnCoSo.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;

namespace DoAnCoSo.Services
{
    public class PostService
    {
        private readonly ApplicationDbContext _context;
        private readonly AIEmbeddingService _aiService;

        public PostService(ApplicationDbContext context, AIEmbeddingService aiService)
        {
            _context = context;
            _aiService = aiService;
        }

        /// <summary>
        /// Sinh vector embedding cho bài viết mới dựa trên nội dung.
        /// </summary>
        public async Task GeneratePostEmbeddingAsync(Post post)
        {
            var text = post.Content ?? "";
            var embedding = await _aiService.GenerateEmbeddingAsync(text);

            // Chuyển float[] thành chuỗi để lưu vào DB
            post.EmbeddingVector = string.Join(",", embedding.Select(v => v.ToString(System.Globalization.CultureInfo.InvariantCulture)));


            _context.Posts.Update(post);
            await _context.SaveChangesAsync();
        }

        // Gợi ý bài viết phù hợp
        public async Task<List<int>> GetRelevantPostIdsAsync(string userId)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);

            // Kiểm tra user có vector chưa
            if (user == null || string.IsNullOrEmpty(user.EmbeddingVector))
            {
                Console.WriteLine($"User {userId} chưa có Vector sở thích.");
                return new List<int>();
            }

            try
            {
                // ✅ FIX LỖI: Thêm CultureInfo.InvariantCulture để hiểu dấu chấm
                var userVector = user.EmbeddingVector
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(v => float.Parse(v, CultureInfo.InvariantCulture))
                    .ToArray();

                var posts = await _context.Posts
                    .AsNoTracking() // Tối ưu tốc độ
                    .Where(p => !string.IsNullOrEmpty(p.EmbeddingVector))
                    .Select(p => new { p.Id, p.EmbeddingVector }) // Chỉ lấy trường cần thiết
                    .ToListAsync();

                var similarPosts = posts
                    .Select(p =>
                    {
                        // ✅ FIX LỖI: Thêm CultureInfo.InvariantCulture ở đây nữa
                        var postVec = p.EmbeddingVector
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(v => float.Parse(v, CultureInfo.InvariantCulture))
                            .ToArray();

                        var score = CosineSimilarity(userVector, postVec);
                        return new { p.Id, Score = score };
                    })
                    .Where(x => x.Score > 0.3) // (Tùy chọn) Chỉ lấy bài có độ giống > 30%
                    .OrderByDescending(p => p.Score) // Sắp xếp điểm cao nhất lên đầu
                    .Take(10)
                    .Select(p => p.Id)
                    .ToList();

                Console.WriteLine($"Tìm thấy {similarPosts.Count} bài viết phù hợp cho user {userId}");
                return similarPosts;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi tính toán vector: " + ex.Message);
                return new List<int>();
            }
        }


        private double CosineSimilarity(float[] vecA, float[] vecB)
        {
            if (vecA.Length != vecB.Length) return 0;
            double dot = 0, magA = 0, magB = 0;
            for (int i = 0; i < vecA.Length; i++)
            {
                dot += vecA[i] * vecB[i];
                magA += vecA[i] * vecA[i];
                magB += vecB[i] * vecB[i];
            }
            return dot / (Math.Sqrt(magA) * Math.Sqrt(magB));
        }

    }
}

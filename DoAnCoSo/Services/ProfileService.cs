using DoAnCoSo.Models;
using Microsoft.EntityFrameworkCore;

namespace DoAnCoSo.Services
{
    public class ProfileService
    {
        private readonly ApplicationDbContext _context;
        private readonly AIEmbeddingService _aiService;

        public ProfileService(ApplicationDbContext context, AIEmbeddingService aiService)
        {
            _context = context;
            _aiService = aiService;
        }

        /// <summary>
        /// Sinh vector embedding cho hồ sơ người dùng dựa trên mô tả, chuyên ngành và ngành học.
        /// </summary>
        public async Task GenerateUserEmbeddingAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            // 🔹 Ghép các phần mô tả thành 1 đoạn văn để gửi lên AI
            var text = $"{user.FullName} {user.Major} {user.Describe} {user.Description}".Trim();

            if (string.IsNullOrWhiteSpace(text))
                return;

            var embedding = await _aiService.GenerateEmbeddingAsync(text);

            // Chuyển vector thành chuỗi để lưu DB
            user.EmbeddingVector = string.Join(",", embedding.Select(v => v.ToString(System.Globalization.CultureInfo.InvariantCulture)));

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Gợi ý người dùng có hồ sơ tương đồng nhất (theo vector embedding).
        /// </summary>
        public async Task<List<ApplicationUser>> GetRelevantUsersAsync(string userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null || string.IsNullOrEmpty(user.EmbeddingVector))
                return new List<ApplicationUser>();

            var userVector = user.EmbeddingVector
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => float.Parse(s, System.Globalization.CultureInfo.InvariantCulture))
                .ToArray();

            var allUsers = await _context.Users
                .Where(u => u.Id != userId && !string.IsNullOrEmpty(u.EmbeddingVector))
                .ToListAsync();

            var similarUsers = allUsers
                .Select(u => new
                {
                    User = u,
                    Score = CosineSimilarity(
                        userVector,
                        u.EmbeddingVector
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => float.Parse(s, System.Globalization.CultureInfo.InvariantCulture))
                            .ToArray())
                })
                .OrderByDescending(x => x.Score)
                .Take(10)
                .Select(x => x.User)
                .ToList();

            return similarUsers;
        }

        /// <summary>
        /// Tính độ tương đồng cosine giữa 2 vector
        /// </summary>
        public static double CosineSimilarity(float[] vecA, float[] vecB)
        {
            if (vecA.Length != vecB.Length) return 0;

            double dot = 0.0, magA = 0.0, magB = 0.0;
            for (int i = 0; i < vecA.Length; i++)
            {
                dot += vecA[i] * vecB[i];
                magA += Math.Pow(vecA[i], 2);
                magB += Math.Pow(vecB[i], 2);
            }

            return dot / (Math.Sqrt(magA) * Math.Sqrt(magB));
        }
        public async Task<List<string>> GetSimilarUsersAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return new List<string>();

            var relevantUsers = await GetRelevantUsersAsync(userId);
            return relevantUsers.Select(u => u.Id).ToList();
        }

    }
}

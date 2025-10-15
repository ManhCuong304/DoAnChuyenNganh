using DoAnCoSo.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DoAnCoSo.Services
{
    public class PostService
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;

        public PostService(ApplicationDbContext context, HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }

        public async Task<HashSet<int>> GetRelevantPostIdsAsync(string userId)
        {
            var profile = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new {
                    Description = u.Description ?? "",
                    Major = u.Major ?? "",
                    Describe = u.Describe ?? ""
                })
                .FirstOrDefaultAsync();

            if (profile == null)
                return new HashSet<int>();

            var posts = await _context.Posts
                .OrderByDescending(p => p.CreatedAt)
                .Take(20)
                .Select(p => new { p.Id, p.Content })
                .ToListAsync();

            var promptBuilder = new System.Text.StringBuilder();
            promptBuilder.AppendLine("Bạn là một AI giúp gợi ý bài viết không phù hợp cho người dùng dựa trên thông tin sau:");

            promptBuilder.AppendLine("Thông tin user:");
          
            promptBuilder.AppendLine($"- Description: {profile.Description}");
            promptBuilder.AppendLine($"- Major: {profile.Major}");
            promptBuilder.AppendLine($"- Describe: {profile.Describe}");
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("Danh sách bài viết (ID: Nội dung):");

            foreach (var post in posts)
            {
                var shortContent = post.Content.Length > 300 ? post.Content.Substring(0, 300) + "..." : post.Content;
                shortContent = Regex.Replace(shortContent, @"\s+", " ").Trim();
                promptBuilder.AppendLine($"{post.Id}: {shortContent}");
            }

            promptBuilder.AppendLine();
            promptBuilder.AppendLine("Hãy trả về danh sách ID bài viết phù hợp nhất, cách nhau bằng dấu phẩy, không có thêm bất kỳ chữ hoặc ký tự nào khác.");

            var prompt = promptBuilder.ToString();

            var requestData = new
            {
                model = "openai/gpt-4o-mini",
                temperature = 0.7,
                messages = new[]
                {
            new { role = "user", content = prompt }
        }
            };

            var response = await _httpClient.PostAsJsonAsync("chat/completions", requestData);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"OpenRouter API error: {response.StatusCode} - {errorContent}");
            }

            var json = await response.Content.ReadAsStringAsync();

            try
            {
                using var doc = JsonDocument.Parse(json);
                var choices = doc.RootElement.GetProperty("choices");
                var messageContent = choices[0].GetProperty("message").GetProperty("content").GetString();

                Console.WriteLine("AI trả về: " + messageContent);

                var cleaned = Regex.Replace(messageContent ?? "", @"[^\d,]", "");
                var ids = cleaned.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => int.TryParse(s.Trim(), out int id) ? id : (int?)null)
                    .Where(id => id.HasValue)
                    .Select(id => id.Value)
                    .ToHashSet();

                // 🔹 Fallback: nếu AI không trả về gì thì tìm bài viết chứa Major của user
                if (!ids.Any() && !string.IsNullOrWhiteSpace(profile.Major))
                {
                    var fallbackPosts = await _context.Posts
                        .Where(p => p.Content.Contains(profile.Major))
                        .Select(p => p.Id)
                        .ToListAsync();

                    ids.UnionWith(fallbackPosts);
                }

                return ids;
            }
            catch (JsonException ex)
            {
                throw new Exception($"JSON parse error: {ex.Message}\nResponse content: {json}");
            }
        }

    }
}

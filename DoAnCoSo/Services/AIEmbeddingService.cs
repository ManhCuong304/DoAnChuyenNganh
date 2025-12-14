using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class AIEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public AIEmbeddingService(IConfiguration config)
    {
        _httpClient = new HttpClient();
        _apiKey = config["OpenRouter:ApiKey"]; // hoặc tên key bạn dùng
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        var requestBody = new
        {
            model = "text-embedding-3-small", // hoặc model embedding bạn chọn
            input = text
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _apiKey);

        var response = await _httpClient.PostAsync("https://openrouter.ai/api/v1/embeddings", content);
        Console.WriteLine($"Embedding API status: {response.StatusCode}");
        var respText = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Response: {respText}");
        response.EnsureSuccessStatusCode();

        // Đọc phản hồi JSON
        var responseJson = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(responseJson);
        var embeddingArray = doc.RootElement
            .GetProperty("data")[0]
            .GetProperty("embedding")
            .EnumerateArray()
            .Select(x => x.GetSingle())
            .ToArray();

        return embeddingArray;
    }

    // Hàm tính cosine similarity
    public static float CosineSimilarity(float[] v1, float[] v2)
    {
        if (v1.Length != v2.Length) return 0;

        float dot = 0, mag1 = 0, mag2 = 0;
        for (int i = 0; i < v1.Length; i++)
        {
            dot += v1[i] * v2[i];
            mag1 += v1[i] * v1[i];
            mag2 += v2[i] * v2[i];
        }

        return dot / ((float)Math.Sqrt(mag1) * (float)Math.Sqrt(mag2));
    }
}

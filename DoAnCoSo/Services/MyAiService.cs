using System.Net.Http.Json;

namespace DoAnCoSo.Services
{
    public class MyAiService
    {
        private readonly HttpClient _http;

        public MyAiService(HttpClient http)
        {
            _http = http;

            if (_http.BaseAddress == null)
                _http.BaseAddress = new Uri("http://127.0.0.1:8000/");
        }

        public async Task<string> Summarize(string text)
        {
            try
            {
                var body = new { text };

                // ⭐ THAY ĐỔI NHẸ NHẤT CÓ THỂ — vẫn giữ nguyên logic của bạn
                var response = await _http.PostAsJsonAsync("summarize", body);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return $"Lỗi gọi API Python: {response.StatusCode}. Chi tiết: {errorContent}";
                }

                var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

                if (result != null && result.ContainsKey("summary"))
                    return result["summary"];
                else
                    return "API Python trả về dữ liệu không hợp lệ";
            }
            catch (Exception ex)
            {
                return $"Lỗi khi gọi API Python: {ex.Message}";
            }
        }

    }
}

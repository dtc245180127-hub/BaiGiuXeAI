using System.Text;
using System.Text.Json;
using QuanLyBaiXe.ViewModels;

namespace QuanLyBaiXe.Services
{
    public class AIAnalysisService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public AIAnalysisService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<AIAnalysisViewModel> AnalyzeAsync(object reportData)
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            var model = _configuration["Gemini:Model"] ?? "gemini-2.5-flash";

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new Exception("Chưa cấu hình Gemini API key.");
            }

            var systemPrompt = """
                Bạn là trợ lý AI hỗ trợ phân tích dữ liệu cho hệ thống quản lý bãi xe.

                Nhiệm vụ:
                - Chỉ phân tích các dữ liệu được cung cấp trong JSON.
                - Không tự ý tạo ra dữ liệu không có trong đầu vào.
                - Không thay đổi dữ liệu cơ sở dữ liệu.
                - Không thay đổi mức giá.
                - Không tự ý phân công nhân sự.
                - Đưa ra nhận định ngắn gọn, rõ ràng và phù hợp với dữ liệu.

                Kết quả bắt buộc trả về dưới dạng JSON với cấu trúc:

                {
                  "status": "success",
                  "summary": "Tóm tắt tình hình",
                  "insights": [
                    "Nhận định 1",
                    "Nhận định 2"
                  ],
                  "recommendations": [
                    "Đề xuất 1",
                    "Đề xuất 2"
                  ]
                }

                Chỉ trả về JSON, không thêm markdown hoặc giải thích bên ngoài JSON.
                """;

            var inputJson = JsonSerializer.Serialize(
                reportData,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            var prompt = $"""
                {systemPrompt}

                Dữ liệu báo cáo cần phân tích:
                {inputJson}
                """;

            var payload = new
            {
                systemInstruction = new
                {
                    parts = new[]
                    {
                        new
                        {
                            text = systemPrompt
                        }
                    }
                },
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[]
                        {
                            new
                            {
                                text = inputJson
                            }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(payload);

            var client = _httpClientFactory.CreateClient();

            var url =
                $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

            using var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync(url, content);

            var responseText =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Gọi Gemini API thất bại: {responseText}");
            }

            using var document =
                JsonDocument.Parse(responseText);

            var root = document.RootElement;

            if (!root.TryGetProperty("candidates", out var candidates) ||
                candidates.GetArrayLength() == 0)
            {
                throw new Exception(
                    "Gemini không trả về kết quả phân tích.");
            }

            var candidate = candidates[0];

            if (!candidate.TryGetProperty("content", out var contentNode) ||
                !contentNode.TryGetProperty("parts", out var parts))
            {
                throw new Exception(
                    "Không đọc được nội dung phản hồi từ Gemini.");
            }

            var aiText = new StringBuilder();

            foreach (var part in parts.EnumerateArray())
            {
                if (part.TryGetProperty("text", out var textNode))
                {
                    aiText.Append(textNode.GetString());
                }
            }

            var resultText = aiText.ToString().Trim();

            if (string.IsNullOrWhiteSpace(resultText))
            {
                throw new Exception(
                    "Gemini trả về nội dung rỗng.");
            }

            // Loại bỏ markdown code fence nếu Gemini trả về ```json ... ```
            resultText = resultText
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            var result =
                JsonSerializer.Deserialize<AIAnalysisViewModel>(
                    resultText,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (result == null)
            {
                throw new Exception(
                    "Không thể chuyển kết quả AI thành dữ liệu phân tích.");
            }

            return result;
        }
    }
}
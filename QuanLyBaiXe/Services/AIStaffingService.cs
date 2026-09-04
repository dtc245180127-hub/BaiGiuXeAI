
using System.Text;
using System.Text.Json;
using QuanLyBaiXe.ViewModels;

namespace QuanLyBaiXe.Services
{
    public class AIStaffingService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public AIStaffingService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<AIStaffingViewModel> RecommendAsync(object staffingData)
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            var model = _configuration["Gemini:Model"] ?? "gemini-2.5-flash";

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new Exception("Chưa cấu hình Gemini API key.");
            }

            var systemPrompt = """
                Bạn là trợ lý AI hỗ trợ quản lý nhân sự cho hệ thống quản lý bãi xe.

                Nhiệm vụ:
                - Phân tích dữ liệu lưu lượng xe được cung cấp trong JSON.
                - Xác định các khung giờ có lưu lượng xe cao.
                - Đưa ra đề xuất bố trí nhân sự phù hợp với tình hình thực tế.
                - Chỉ sử dụng dữ liệu có trong JSON.
                - Không tự ý tạo ra số liệu hoặc nhân sự không có trong dữ liệu.
                - Không tự ý phân công một nhân viên cụ thể.
                - Không tạo hoặc thay đổi ca làm việc trong cơ sở dữ liệu.
                - Không thay đổi dữ liệu hệ thống.
                - Đề xuất chỉ mang tính chất hỗ trợ để quản lý xem xét và quyết định.

                Khi đưa ra đề xuất, cần chú ý:
                - Khung giờ có lưu lượng xe cao cần được ưu tiên tăng cường nhân sự.
                - Không đề xuất số nhân viên vượt quá số nhân viên hiện có trong dữ liệu.
                - Nếu lưu lượng thấp thì có thể đề xuất duy trì nhân sự hiện tại.
                - Nếu dữ liệu không đủ để đưa ra nhận định thì phải nói rõ.

                Kết quả bắt buộc trả về JSON với cấu trúc:

                {
                  "status": "success",
                  "summary": "Tóm tắt tình hình nhân sự và lưu lượng xe",
                  "peakHours": [
                    "Khung giờ cao điểm"
                  ],
                  "recommendations": [
                    "Đề xuất bố trí nhân sự"
                  ],
                  "notes": [
                    "Lưu ý cho quản lý"
                  ],
                  "availableStaff": 0,
                  "currentVehicles": 0
                }

                Chỉ trả về JSON, không thêm markdown hoặc giải thích bên ngoài JSON.
                """;

            var inputJson = JsonSerializer.Serialize(
                staffingData,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

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
                    "Gemini không trả về kết quả đề xuất nhân sự.");
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

            resultText = resultText
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            var result =
                JsonSerializer.Deserialize<AIStaffingViewModel>(
                    resultText,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (result == null)
            {
                throw new Exception(
                    "Không thể chuyển kết quả AI thành dữ liệu đề xuất nhân sự.");
            }

            return result;
        }
    }
}

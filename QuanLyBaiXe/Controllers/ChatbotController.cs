using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace QuanLyBaiXe.Controllers
{
    [Route("[controller]")]
    public class ChatbotController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public ChatbotController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { error = "Nội dung tin nhắn đang trống." });
            }

            var apiKey = _configuration["Gemini:ApiKey"];
            var model = _configuration["Gemini:Model"] ?? "gemini-2.5-flash";

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return BadRequest(new { error = "Chưa cấu hình Gemini API key." });
            }

            var client = _httpClientFactory.CreateClient();
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

            var contents = new List<object>();

            if (request.History != null && request.History.Count > 0)
            {
                foreach (var item in request.History)
                {
                    if (string.IsNullOrWhiteSpace(item.Text)) continue;

                    var role = item.Role?.Trim().ToLower() == "model" ? "model" : "user";

                    contents.Add(new
                    {
                        role,
                        parts = new object[]
                        {
                            new { text = item.Text }
                        }
                    });
                }
            }

            contents.Add(new
            {
                role = "user",
                parts = new object[]
                {
                    new { text = request.Message }
                }
            });

            var payload = new
            {
                systemInstruction = new
                {
                    parts = new object[]
                    {
                        new
                        {
                            text = "Bạn là trợ lý AI cho website quản lý bãi xe. Hãy trả lời bằng tiếng Việt, rõ ràng, ngắn gọn, dễ hiểu. Nếu người dùng hỏi về chức năng website thì hãy giới thiệu theo hướng trợ lý cho hệ thống quản lý bãi xe."
                        }
                    }
                },
                contents
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                // Retry tối đa 3 lần nếu model bị quá tải tạm thời
                for (int attempt = 1; attempt <= 3; attempt++)
                {
                    Console.WriteLine("===== GEMINI REQUEST =====");
                    Console.WriteLine($"Attempt: {attempt}");
                    Console.WriteLine($"Model: {model}");
                    Console.WriteLine(json);

                    var response = await client.PostAsync(url, content);
                    var responseText = await response.Content.ReadAsStringAsync();

                    Console.WriteLine("===== GEMINI RESPONSE =====");
                    Console.WriteLine($"Status: {(int)response.StatusCode} - {response.StatusCode}");
                    Console.WriteLine(responseText);

                    if (response.IsSuccessStatusCode)
                    {
                        using var doc = JsonDocument.Parse(responseText);
                        var root = doc.RootElement;

                        if (root.TryGetProperty("candidates", out var candidates) &&
                            candidates.GetArrayLength() > 0)
                        {
                            var candidate = candidates[0];

                            if (candidate.TryGetProperty("content", out var contentNode) &&
                                contentNode.TryGetProperty("parts", out var parts))
                            {
                                var sb = new StringBuilder();

                                foreach (var part in parts.EnumerateArray())
                                {
                                    if (part.TryGetProperty("text", out var textNode))
                                    {
                                        sb.Append(textNode.GetString());
                                    }
                                }

                                var reply = sb.ToString().Trim();

                                return Ok(new
                                {
                                    reply = string.IsNullOrWhiteSpace(reply)
                                        ? "AI chưa trả về nội dung."
                                        : reply
                                });
                            }
                        }

                        return StatusCode(500, new
                        {
                            error = "Không đọc được phản hồi từ Gemini.",
                            detail = responseText,
                            model
                        });
                    }

                    // Nếu quá tải tạm thời thì đợi rồi thử lại
                    if ((int)response.StatusCode == 503 && attempt < 3)
                    {
                        await Task.Delay(1200 * attempt);
                        continue;
                    }

                    return StatusCode((int)response.StatusCode, new
                    {
                        error = "Gọi Gemini API thất bại.",
                        detail = responseText,
                        model
                    });
                }

                return StatusCode(503, new
                {
                    error = "Gemini đang quá tải, vui lòng thử lại sau.",
                    model
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("===== GEMINI EXCEPTION =====");
                Console.WriteLine(ex);

                return StatusCode(500, new
                {
                    error = ex.Message,
                    detail = ex.InnerException?.Message
                });
            }
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
        public List<ChatHistoryItem> History { get; set; } = new();
    }

    public class ChatHistoryItem
    {
        public string Role { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
}
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiXe.Data;

namespace QuanLyBaiXe.Controllers
{
    [Route("[controller]")]
    public class ChatbotController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public ChatbotController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            AppDbContext context)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage(
            [FromBody] ChatRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new
                {
                    error = "Nội dung tin nhắn đang trống."
                });
            }

            var apiKey =
                _configuration["Gemini:ApiKey"];

            var model =
                _configuration["Gemini:Model"]
                ?? "gemini-2.5-flash";

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return BadRequest(new
                {
                    error = "Chưa cấu hình Gemini API key."
                });
            }

            try
            {
                // =====================================================
                // 1. KIỂM TRA NGƯỜI DÙNG ĐANG ĐĂNG NHẬP
                // =====================================================

                var username =
                    HttpContext.Session.GetString("Username");

                var role =
                    HttpContext.Session.GetString("Role");

                if (string.IsNullOrWhiteSpace(username))
                {
                    return Unauthorized(new
                    {
                        error = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại."
                    });
                }


                // =====================================================
                // 2. LẤY DỮ LIỆU TỪ SQL SERVER
                // =====================================================

                var now = DateTime.Now;

                var today = now.Date;

                var tomorrow =
                    today.AddDays(1);

                var sevenDaysAgo =
                    today.AddDays(-6);


                // -----------------------------------------------------
                // Lấy các phiên gửi xe
                // -----------------------------------------------------

                var parkingSessions =
                    await _context.ParkingSessions
                        .Include(x => x.Vehicle)
                        .AsNoTracking()
                        .ToListAsync();


                // -----------------------------------------------------
                // Xe đang gửi trong bãi
                // -----------------------------------------------------

                var currentSessions =
                    parkingSessions
                        .Where(x =>
                            x.CheckOutTime == null &&
                            x.Status == "DangGui")
                        .ToList();


                // -----------------------------------------------------
                // Xe vào trong ngày hôm nay
                // -----------------------------------------------------

                var todayCheckIns =
                    parkingSessions
                        .Where(x =>
                            x.CheckInTime >= today &&
                            x.CheckInTime < tomorrow)
                        .ToList();


                // -----------------------------------------------------
                // Xe đã ra trong ngày hôm nay
                // -----------------------------------------------------

                var todayCheckOuts =
                    parkingSessions
                        .Where(x =>
                            x.CheckOutTime.HasValue &&
                            x.CheckOutTime.Value >= today &&
                            x.CheckOutTime.Value < tomorrow)
                        .ToList();


                // -----------------------------------------------------
                // Doanh thu trong ngày hôm nay
                // -----------------------------------------------------

                var todayRevenue =
                    todayCheckOuts
                        .Where(x => x.Fee.HasValue)
                        .Sum(x => x.Fee!.Value);


                // -----------------------------------------------------
                // Doanh thu 7 ngày gần nhất
                // -----------------------------------------------------

                var recentCheckOuts =
                    parkingSessions
                        .Where(x =>
                            x.CheckOutTime.HasValue &&
                            x.CheckOutTime.Value >= sevenDaysAgo &&
                            x.CheckOutTime.Value < tomorrow)
                        .ToList();


                var recentRevenue =
                    recentCheckOuts
                        .Where(x => x.Fee.HasValue)
                        .Sum(x => x.Fee!.Value);


                // =====================================================
                // 3. THỐNG KÊ KHUNG GIỜ ĐÔNG
                // =====================================================

                var hourlyTraffic =
                    todayCheckIns
                        .GroupBy(x => x.CheckInTime.Hour)
                        .Select(g => new
                        {
                            Hour = g.Key,
                            VehicleCount = g.Count()
                        })
                        .OrderByDescending(x => x.VehicleCount)
                        .ThenBy(x => x.Hour)
                        .ToList();


                var peakHour =
                    hourlyTraffic.FirstOrDefault();


                // =====================================================
                // 4. THỐNG KÊ LOẠI XE
                // =====================================================

                var vehicleTypeStatistics =
                    currentSessions
                        .Where(x =>
                            x.Vehicle != null &&
                            !string.IsNullOrWhiteSpace(
                                x.Vehicle.VehicleType))
                        .GroupBy(x =>
                            x.Vehicle!.VehicleType)
                        .Select(g => new
                        {
                            VehicleType = g.Key,
                            Count = g.Count()
                        })
                        .OrderByDescending(x => x.Count)
                        .ToList();


                // =====================================================
                // 5. THỐNG KÊ XE VÀO TRONG 7 NGÀY
                // =====================================================

                var dailyTraffic =
                    parkingSessions
                        .Where(x =>
                            x.CheckInTime >= sevenDaysAgo &&
                            x.CheckInTime < tomorrow)
                        .GroupBy(x =>
                            x.CheckInTime.Date)
                        .Select(g => new
                        {
                            Date =
                                g.Key.ToString("yyyy-MM-dd"),

                            VehicleCount =
                                g.Count()
                        })
                        .OrderBy(x => x.Date)
                        .ToList();


                // =====================================================
                // 6. THỐNG KÊ VÉ THÁNG
                // =====================================================

                var monthlyTickets =
                    await _context.MonthlyTickets
                        .AsNoTracking()
                        .ToListAsync();


                var activeMonthlyTickets =
                    monthlyTickets
                        .Count(x =>
                            x.EndDate.Date >= today &&
                            x.Status == "ConHan");


                var expiredMonthlyTickets =
                    monthlyTickets
                        .Count(x =>
                            x.EndDate.Date < today);


                // =====================================================
                // 7. THỐNG KÊ MẤT VÉ
                // =====================================================

                var todayLostTickets =
                    todayCheckIns
                        .Count(x =>
                            x.IsLostTicket);


                var totalLostTickets =
                    parkingSessions
                        .Count(x =>
                            x.IsLostTicket);


                // =====================================================
                // 8. THỐNG KÊ HOẠT ĐỘNG HỆ THỐNG
                // =====================================================

                var todayActivityLogs =
                    await _context.ActivityLogs
                        .AsNoTracking()
                        .Where(x =>
                            x.CreatedAt >= today &&
                            x.CreatedAt < tomorrow)
                        .Select(x => new
                        {
                            x.Role,
                            x.ActionName
                        })
                        .ToListAsync();


                var activityStatistics =
                    todayActivityLogs
                        .GroupBy(x => x.ActionName)
                        .Select(g => new
                        {
                            Action = g.Key,
                            Count = g.Count()
                        })
                        .OrderByDescending(x => x.Count)
                        .ToList();


                // =====================================================
                // 9. TẠO JSON DỮ LIỆU CHO AI
                // =====================================================

                var reportData = new
                {
                    request_date =
                        today.ToString("yyyy-MM-dd"),

                    user_role =
                        role,

                    parking = new
                    {
                        current_vehicle_count =
                            currentSessions.Count,

                        today_check_in_count =
                            todayCheckIns.Count,

                        today_check_out_count =
                            todayCheckOuts.Count,

                        today_revenue =
                            todayRevenue,

                        seven_day_revenue =
                            recentRevenue,

                        total_lost_ticket_count =
                            totalLostTickets,

                        today_lost_ticket_count =
                            todayLostTickets
                    },

                    peak_hour = peakHour == null
                        ? null
                        : new
                        {
                            hour =
                                $"{peakHour.Hour:00}:00",

                            vehicle_count =
                                peakHour.VehicleCount
                        },

                    hourly_traffic =
                        hourlyTraffic
                            .Select(x => new
                            {
                                hour =
                                    $"{x.Hour:00}:00",

                                vehicle_count =
                                    x.VehicleCount
                            })
                            .ToList(),

                    daily_traffic =
                        dailyTraffic,

                    vehicle_types =
                        vehicleTypeStatistics
                            .Select(x => new
                            {
                                vehicle_type =
                                    x.VehicleType,

                                count =
                                    x.Count
                            })
                            .ToList(),

                    monthly_tickets = new
                    {
                        total =
                            monthlyTickets.Count,

                        active =
                            activeMonthlyTickets,

                        expired =
                            expiredMonthlyTickets
                    },

                    system_activity =
                        activityStatistics
                            .Select(x => new
                            {
                                action =
                                    x.Action,

                                count =
                                    x.Count
                            })
                            .ToList()
                };


                var inputJson =
                    JsonSerializer.Serialize(
                        reportData,
                        new JsonSerializerOptions
                        {
                            WriteIndented = true
                        });


                // =====================================================
                // 10. SYSTEM PROMPT
                // =====================================================

                var systemPrompt = """
                    Bạn là trợ lý AI của hệ thống quản lý bãi xe.

                    Nhiệm vụ:
                    - Trả lời câu hỏi của người dùng bằng tiếng Việt.
                    - Khi câu hỏi liên quan đến dữ liệu bãi xe, phải dựa trên dữ liệu JSON được cung cấp.
                    - Không được tự tạo hoặc suy đoán số liệu không có trong JSON.
                    - Nếu dữ liệu không đủ để trả lời chính xác, phải nói rõ rằng dữ liệu hiện tại chưa đủ.
                    - Có thể thực hiện các phép tính đơn giản dựa trên số liệu được cung cấp.
                    - Có thể giải thích xu hướng, thống kê và khung giờ đông dựa trên dữ liệu.
                    - Không được thay đổi dữ liệu trong cơ sở dữ liệu.
                    - Không được tự ý thay đổi mức giá.
                    - Không được tự ý phân công nhân viên.
                    - Nếu được hỏi về bố trí nhân sự, chỉ đưa ra đề xuất tham khảo dựa trên khung giờ và lưu lượng xe.
                    - Không tiết lộ mật khẩu, API key, connection string hoặc thông tin bảo mật.
                    - Trả lời ngắn gọn, rõ ràng và dễ hiểu.
                    """;


                // =====================================================
                // 11. TẠO NỘI DUNG GỬI GEMINI
                // =====================================================

                var contents =
                    new List<object>();


                // Lịch sử hội thoại
                if (request.History != null &&
                    request.History.Count > 0)
                {
                    foreach (
                        var item
                        in request.History)
                    {
                        if (
                            string.IsNullOrWhiteSpace(
                                item.Text))
                        {
                            continue;
                        }


                        var historyRole =
                            item.Role?
                                .Trim()
                                .ToLower()
                                == "model"
                                    ? "model"
                                    : "user";


                        contents.Add(
                            new
                            {
                                role =
                                    historyRole,

                                parts =
                                    new object[]
                                    {
                                        new
                                        {
                                            text =
                                                item.Text
                                        }
                                    }
                            });
                    }
                }


                // -----------------------------------------------------
                // Câu hỏi hiện tại + dữ liệu SQL Server
                // -----------------------------------------------------

                var userPrompt = $"""
                    Dữ liệu hiện tại của hệ thống:
                    {inputJson}

                    Câu hỏi của người dùng:
                    {request.Message}

                    Hãy trả lời câu hỏi dựa trên dữ liệu được cung cấp.
                    """;


                contents.Add(
                    new
                    {
                        role = "user",

                        parts =
                            new object[]
                            {
                                new
                                {
                                    text =
                                        userPrompt
                                }
                            }
                    });


                // =====================================================
                // 12. PAYLOAD GỬI GEMINI
                // =====================================================

                var payload = new
                {
                    systemInstruction = new
                    {
                        parts =
                            new object[]
                            {
                                new
                                {
                                    text =
                                        systemPrompt
                                }
                            }
                    },

                    contents
                };


                var json =
                    JsonSerializer.Serialize(
                        payload);


                var client =
                    _httpClientFactory.CreateClient();


                var url =
                    $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";


                // =====================================================
                // 13. GỌI GEMINI
                // =====================================================

                for (
                    int attempt = 1;
                    attempt <= 3;
                    attempt++)
                {
                    Console.WriteLine(
                        "===== GEMINI CHATBOT REQUEST =====");

                    Console.WriteLine(
                        $"Attempt: {attempt}");

                    Console.WriteLine(
                        $"Model: {model}");


                    using var content =
                        new StringContent(
                            json,
                            Encoding.UTF8,
                            "application/json");


                    var response =
                        await client.PostAsync(
                            url,
                            content);


                    var responseText =
                        await response.Content
                            .ReadAsStringAsync();


                    Console.WriteLine(
                        "===== GEMINI CHATBOT RESPONSE =====");

                    Console.WriteLine(
                        $"Status: {(int)response.StatusCode} - {response.StatusCode}");


                    if (
                        response.IsSuccessStatusCode)
                    {
                        using var document =
                            JsonDocument.Parse(
                                responseText);


                        var root =
                            document.RootElement;


                        if (
                            root.TryGetProperty(
                                "candidates",
                                out var candidates) &&
                            candidates.GetArrayLength() > 0)
                        {
                            var candidate =
                                candidates[0];


                            if (
                                candidate.TryGetProperty(
                                    "content",
                                    out var contentNode) &&
                                contentNode.TryGetProperty(
                                    "parts",
                                    out var parts))
                            {
                                var sb =
                                    new StringBuilder();


                                foreach (
                                    var part
                                    in parts.EnumerateArray())
                                {
                                    if (
                                        part.TryGetProperty(
                                            "text",
                                            out var textNode))
                                    {
                                        sb.Append(
                                            textNode.GetString());
                                    }
                                }


                                var reply =
                                    sb.ToString().Trim();


                                return Ok(
                                    new
                                    {
                                        reply =
                                            string.IsNullOrWhiteSpace(
                                                reply)
                                                    ? "AI chưa trả về nội dung."
                                                    : reply
                                    });
                            }
                        }


                        return StatusCode(
                            500,
                            new
                            {
                                error =
                                    "Không đọc được phản hồi từ Gemini.",

                                detail =
                                    responseText
                            });
                    }


                    // =================================================
                    // GEMINI QUÁ TẢI TẠM THỜI
                    // =================================================

                    if (
                        (int)response.StatusCode == 503 &&
                        attempt < 3)
                    {
                        await Task.Delay(
                            1200 * attempt);

                        continue;
                    }


                    return StatusCode(
                        (int)response.StatusCode,
                        new
                        {
                            error =
                                "Gọi Gemini API thất bại.",

                            detail =
                                responseText
                        });
                }


                return StatusCode(
                    503,
                    new
                    {
                        error =
                            "Gemini đang quá tải, vui lòng thử lại sau."
                    });
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "===== GEMINI CHATBOT EXCEPTION =====");

                Console.WriteLine(ex);


                return StatusCode(
                    500,
                    new
                    {
                        error =
                            "Có lỗi xảy ra khi xử lý yêu cầu AI.",

                        detail =
                            ex.Message
                    });
            }
        }
    }


    // =============================================================
    // REQUEST MODEL
    // =============================================================

    public class ChatRequest
    {
        public string Message { get; set; }
            = string.Empty;

        public List<ChatHistoryItem> History { get; set; }
            = new();
    }


    public class ChatHistoryItem
    {
        public string Role { get; set; }
            = string.Empty;

        public string Text { get; set; }
            = string.Empty;
    }
}
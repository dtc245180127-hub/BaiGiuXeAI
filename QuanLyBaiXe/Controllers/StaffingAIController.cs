using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiXe.Data;
using QuanLyBaiXe.Services;

namespace QuanLyBaiXe.Controllers
{
    public class StaffingAIController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly AIStaffingService _aiStaffingService;

        public StaffingAIController(
            AppDbContext context,
            AIStaffingService aiStaffingService) : base(context)
        {
            _context = context;
            _aiStaffingService = aiStaffingService;
        }

        private bool IsManager()
        {
            return HttpContext.Session.GetString("Role") == "QuanLy";
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (!IsManager())
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Recommend()
        {
            if (!IsManager())
            {
                return Unauthorized(new
                {
                    error = "Bạn không có quyền sử dụng chức năng đề xuất nhân sự bằng AI."
                });
            }

            try
            {
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);

                // ==========================================
                // 1. ĐẾM SỐ NHÂN VIÊN HIỆN CÓ
                // ==========================================

                var availableStaff = await _context.Users
                    .AsNoTracking()
                    .Where(u =>
                        u.Role != null &&
                        u.Role.RoleName == "NhanVien")
                    .CountAsync();

                // ==========================================
                // 2. ĐẾM XE ĐANG CÓ TRONG BÃI
                // ==========================================

                var currentVehicles = await _context.ParkingSessions
                    .AsNoTracking()
                    .Where(p =>
                        p.Status == "DangGui" &&
                        !p.CheckOutTime.HasValue)
                    .CountAsync();

                // ==========================================
                // 3. LẤY DỮ LIỆU XE VÀO HÔM NAY
                // ==========================================

                var todayCheckIns = await _context.ParkingSessions
                    .AsNoTracking()
                    .Where(p =>
                        p.CheckInTime >= today &&
                        p.CheckInTime < tomorrow)
                    .Select(p => new
                    {
                        p.CheckInTime,
                        p.CheckOutTime
                    })
                    .ToListAsync();

                // ==========================================
                // 4. THỐNG KÊ LƯU LƯỢNG THEO TỪNG GIỜ
                // ==========================================

                var hourlyTraffic = Enumerable
                    .Range(0, 24)
                    .Select(hour =>
                    {
                        var checkInCount = todayCheckIns.Count(p =>
                            p.CheckInTime.Hour == hour);

                        var checkOutCount = todayCheckIns.Count(p =>
                            p.CheckOutTime.HasValue &&
                            p.CheckOutTime.Value.Date == today &&
                            p.CheckOutTime.Value.Hour == hour);

                        return new
                        {
                            hour,
                            check_in = checkInCount,
                            check_out = checkOutCount,
                            total = checkInCount + checkOutCount
                        };
                    })
                    .ToList();

                // ==========================================
                // 5. XÁC ĐỊNH GIỜ CAO ĐIỂM
                // ==========================================

                var maxTraffic = hourlyTraffic.Max(x => x.total);

                var peakHours = maxTraffic > 0
                    ? hourlyTraffic
                        .Where(x => x.total == maxTraffic)
                        .Select(x =>
                            $"{x.hour:00}:00 - {(x.hour + 1) % 24:00}:00")
                        .ToList()
                    : new List<string>();

                // ==========================================
                // 6. ĐÓNG GÓI DỮ LIỆU GỬI CHO AI
                // ==========================================

                var staffingData = new
                {
                    request_type = "staffing_recommendation",

                    date = today.ToString("yyyy-MM-dd"),

                    available_staff = availableStaff,

                    current_vehicles_in_parking = currentVehicles,

                    peak_hours = peakHours,

                    hourly_traffic = hourlyTraffic
                };

                // ==========================================
                // 7. GỌI AI
                // ==========================================

                var result =
                    await _aiStaffingService.RecommendAsync(
                        staffingData);

                // ==========================================
                // 8. ĐẢM BẢO DỮ LIỆU THỰC TẾ ĐƯỢC HIỂN THỊ
                // ==========================================

                result.AvailableStaff = availableStaff;

                result.CurrentVehicles = currentVehicles;

                if (result.PeakHours == null ||
                    result.PeakHours.Count == 0)
                {
                    result.PeakHours = peakHours;
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                // Trả lỗi chi tiết để dễ kiểm tra khi chạy thử
                return StatusCode(500, new
                {
                    error = "Không thể thực hiện đề xuất nhân sự bằng AI.",
                    detail = ex.ToString()
                });
            }
        }
    }
}
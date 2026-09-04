using Microsoft.AspNetCore.Mvc;
using QuanLyBaiXe.Data;
using QuanLyBaiXe.Services;
using QuanLyBaiXe.ViewModels;

namespace QuanLyBaiXe.Controllers
{
    public class ReportController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly AIAnalysisService _aiAnalysisService;

        public ReportController(
            AppDbContext context,
            AIAnalysisService aiAnalysisService) : base(context)
        {
            _context = context;
            _aiAnalysisService = aiAnalysisService;
        }

        private bool IsManager()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "QuanLy";
        }

        public IActionResult Index()
        {
            if (!IsManager())
            {
                return RedirectToAction("Index", "Home");
            }

            var today = DateTime.Today;

            var todayRevenue = _context.ParkingSessions
                .Where(p =>
                    p.CheckOutTime.HasValue &&
                    p.CheckOutTime.Value.Date == today &&
                    p.Fee.HasValue)
                .Sum(p => p.Fee ?? 0);

            var totalRevenue = _context.ParkingSessions
                .Where(p =>
                    p.CheckOutTime.HasValue &&
                    p.Fee.HasValue)
                .Sum(p => p.Fee ?? 0);

            var totalVehiclesInParking = _context.ParkingSessions
                .Count(p => p.Status == "DangGui");

            var totalCheckedOutVehicles = _context.ParkingSessions
                .Count(p => p.Status == "DaLay");

            var totalLostTicketVehicles = _context.ParkingSessions
                .Count(p => p.IsLostTicket);

            var totalMonthlyTicketsActive = _context.MonthlyTickets
                .Count(t => t.EndDate >= today);

            var revenueByDates = _context.ParkingSessions
                .Where(p =>
                    p.CheckOutTime.HasValue &&
                    p.Fee.HasValue)
                .AsEnumerable()
                .GroupBy(p => p.CheckOutTime!.Value.Date)
                .Select(g => new RevenueByDateViewModel
                {
                    Date = g.Key,
                    Revenue = g.Sum(x => x.Fee ?? 0)
                })
                .OrderByDescending(x => x.Date)
                .Take(10)
                .OrderBy(x => x.Date)
                .ToList();

            var model = new ReportViewModel
            {
                TodayRevenue = todayRevenue,
                TotalRevenue = totalRevenue,
                TotalVehiclesInParking = totalVehiclesInParking,
                TotalCheckedOutVehicles = totalCheckedOutVehicles,
                TotalLostTicketVehicles = totalLostTicketVehicles,
                TotalMonthlyTicketsActive = totalMonthlyTicketsActive,
                RevenueByDates = revenueByDates
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AnalyzeAI()
        {
            if (!IsManager())
            {
                return Unauthorized(new
                {
                    error = "Bạn không có quyền sử dụng chức năng phân tích AI."
                });
            }

            try
            {
                var today = DateTime.Today;

                var todayRevenue = _context.ParkingSessions
                    .Where(p =>
                        p.CheckOutTime.HasValue &&
                        p.CheckOutTime.Value.Date == today &&
                        p.Fee.HasValue)
                    .Sum(p => p.Fee ?? 0);

                var totalRevenue = _context.ParkingSessions
                    .Where(p =>
                        p.CheckOutTime.HasValue &&
                        p.Fee.HasValue)
                    .Sum(p => p.Fee ?? 0);

                var totalVehiclesInParking = _context.ParkingSessions
                    .Count(p => p.Status == "DangGui");

                var totalCheckedOutVehicles = _context.ParkingSessions
                    .Count(p => p.Status == "DaLay");

                var totalLostTicketVehicles = _context.ParkingSessions
                    .Count(p => p.IsLostTicket);

                var totalMonthlyTicketsActive = _context.MonthlyTickets
                    .Count(t => t.EndDate >= today);

                var revenueByDates = _context.ParkingSessions
                    .Where(p =>
                        p.CheckOutTime.HasValue &&
                        p.Fee.HasValue)
                    .AsEnumerable()
                    .GroupBy(p => p.CheckOutTime!.Value.Date)
                    .Select(g => new
                    {
                        date = g.Key.ToString("yyyy-MM-dd"),
                        revenue = g.Sum(x => x.Fee ?? 0)
                    })
                    .OrderByDescending(x => x.date)
                    .Take(10)
                    .OrderBy(x => x.date)
                    .ToList();

                var reportData = new
                {
                    request_type = "parking_report_analysis",
                    report_date = today.ToString("yyyy-MM-dd"),

                    today_revenue = todayRevenue,
                    total_revenue = totalRevenue,

                    total_vehicles_in_parking =
                        totalVehiclesInParking,

                    total_checked_out_vehicles =
                        totalCheckedOutVehicles,

                    total_lost_ticket_vehicles =
                        totalLostTicketVehicles,

                    total_monthly_tickets_active =
                        totalMonthlyTicketsActive,

                    revenue_by_dates = revenueByDates
                };

                var result =
                    await _aiAnalysisService.AnalyzeAsync(reportData);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Không thể thực hiện phân tích AI.",
                    detail = ex.Message
                });
            }
        }
    }
}
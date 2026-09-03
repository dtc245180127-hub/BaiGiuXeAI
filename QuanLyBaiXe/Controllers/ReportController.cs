using Microsoft.AspNetCore.Mvc;
using QuanLyBaiXe.Data;
using QuanLyBaiXe.ViewModels;

namespace QuanLyBaiXe.Controllers
{
    public class ReportController : BaseController
    {
        private readonly AppDbContext _context;

        public ReportController(AppDbContext context) : base(context)
        {
            _context = context;
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
                .Where(p => p.CheckOutTime.HasValue && p.CheckOutTime.Value.Date == today && p.Fee.HasValue)
                .Sum(p => p.Fee ?? 0);

            var totalRevenue = _context.ParkingSessions
                .Where(p => p.CheckOutTime.HasValue && p.Fee.HasValue)
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
                .Where(p => p.CheckOutTime.HasValue && p.Fee.HasValue)
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
    }
}
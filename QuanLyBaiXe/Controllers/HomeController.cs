using Microsoft.AspNetCore.Mvc;
using QuanLyBaiXe.Data;

namespace QuanLyBaiXe.Controllers
{
    public class HomeController : BaseController
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("Role");

            if (role == "QuanLy")
            {
                return RedirectToAction("AdminDashboard");
            }
            else if (role == "NhanVien")
            {
                return RedirectToAction("StaffDashboard");
            }
            else
            {
                return RedirectToAction("CustomerDashboard");
            }
        }

        public IActionResult AdminDashboard()
        {
            if (HttpContext.Session.GetString("Role") != "QuanLy")
            {
                return RedirectToAction("Index");
            }

            var today = DateTime.Today;

            ViewBag.FullName = HttpContext.Session.GetString("FullName");
            ViewBag.Role = HttpContext.Session.GetString("Role");

            ViewBag.TotalVehiclesInParking = _context.ParkingSessions.Count(p => p.Status == "DangGui");
            ViewBag.TotalMonthlyTicketsActive = _context.MonthlyTickets.Count(t => t.EndDate >= today);
            ViewBag.TodayRevenue = _context.ParkingSessions
                .Where(p => p.CheckOutTime.HasValue && p.CheckOutTime.Value.Date == today && p.Fee.HasValue)
                .Sum(p => p.Fee ?? 0);
            ViewBag.ExpiringTickets = _context.MonthlyTickets
                .Count(t => t.EndDate >= today && t.EndDate <= today.AddDays(7));

            return View();
        }

        public IActionResult StaffDashboard()
        {
            if (HttpContext.Session.GetString("Role") != "NhanVien")
            {
                return RedirectToAction("Index");
            }

            var today = DateTime.Today;

            ViewBag.FullName = HttpContext.Session.GetString("FullName");
            ViewBag.Role = HttpContext.Session.GetString("Role");

            ViewBag.TotalCheckInToday = _context.ParkingSessions.Count(p => p.CheckInTime.Date == today);
            ViewBag.TotalCheckOutToday = _context.ParkingSessions.Count(p => p.CheckOutTime.HasValue && p.CheckOutTime.Value.Date == today);
            ViewBag.TotalMonthlyTicketHandled = _context.MonthlyTickets.Count(t => t.StartDate.Date == today || t.EndDate.Date == today);

            return View();
        }

        public IActionResult CustomerDashboard()
        {
            if (HttpContext.Session.GetString("Role") != "KhachHang")
            {
                return RedirectToAction("Index");
            }

            ViewBag.FullName = HttpContext.Session.GetString("FullName");
            ViewBag.Role = HttpContext.Session.GetString("Role");

            return View();
        }
    }
}
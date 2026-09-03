using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiXe.Data;
using QuanLyBaiXe.Models;

namespace QuanLyBaiXe.Controllers
{
    public class MonthlyTicketController : BaseController
    {
        private readonly AppDbContext _context;
        public IActionResult Index()
        {
            if (!IsStaffOrManager())
            {
                return RedirectToAction("Index", "Home");
            }

            UpdateTicketStatus();

            var tickets = _context.MonthlyTickets
                .Include(t => t.Vehicle)
                .OrderByDescending(t => t.StartDate)
                .ToList();

            return View(tickets);
        }

        public MonthlyTicketController(AppDbContext context) : base(context)
        {
            _context = context;
        }

        private bool IsStaffOrManager()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "NhanVien" || role == "QuanLy";
        }
        private bool IsCustomer()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "KhachHang";
        }
        [HttpGet]
        public IActionResult CustomerCreate()
        {
            if (!IsCustomer())
            {
                return RedirectToAction("Index", "Home");
            }

            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToAction("Login", "Auth");
            }

            int userId = int.Parse(userIdStr);

            ViewBag.Vehicles = _context.Vehicles
                .Where(v => v.UserId == userId)
                .OrderBy(v => v.LicensePlate)
                .ToList();

            return View();
        }
        [HttpPost]
        public IActionResult CustomerCreate(MonthlyTicket ticket)
        {
            if (!IsCustomer())
            {
                return RedirectToAction("Index", "Home");
            }

            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToAction("Login", "Auth");
            }

            int userId = int.Parse(userIdStr);

            var vehicle = _context.Vehicles
                .FirstOrDefault(v => v.Id == ticket.VehicleId && v.UserId == userId);

            if (vehicle == null)
            {
                ModelState.AddModelError("", "Xe không hợp lệ.");
            }

            if (ticket.EndDate <= ticket.StartDate)
            {
                ModelState.AddModelError("", "Ngày hết hạn phải lớn hơn ngày bắt đầu.");
            }

            var activeTicket = _context.MonthlyTickets
                .FirstOrDefault(t => t.VehicleId == ticket.VehicleId && t.Status == "ConHan");

            if (activeTicket != null)
            {
                ModelState.AddModelError("", "Xe này đã có vé tháng còn hạn.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Vehicles = _context.Vehicles
                    .Where(v => v.UserId == userId)
                    .OrderBy(v => v.LicensePlate)
                    .ToList();

                return View(ticket);
            }

            ticket.Price = CalculateMonthlyPrice(ticket.StartDate, ticket.EndDate);
            ticket.Status = ticket.EndDate >= DateTime.Today ? "ConHan" : "HetHan";

            _context.MonthlyTickets.Add(ticket);
            _context.SaveChanges();

            LogActivity("Khách hàng đăng ký vé tháng", $"Khách hàng đăng ký vé tháng cho xe {vehicle?.LicensePlate}.");

            TempData["Success"] = "Đăng ký vé tháng thành công.";
            return RedirectToAction("CustomerRenewList");
        }
        [HttpGet]
        public IActionResult CustomerRenewList()
        {
            if (!IsCustomer())
            {
                return RedirectToAction("Index", "Home");
            }

            UpdateTicketStatus();

            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToAction("Login", "Auth");
            }

            int userId = int.Parse(userIdStr);

            var tickets = _context.MonthlyTickets
                .Include(t => t.Vehicle)
                .Where(t => t.Vehicle != null && t.Vehicle.UserId == userId)
                .OrderByDescending(t => t.StartDate)
                .ToList();

            return View(tickets);
        }
        [HttpGet]
        public IActionResult CustomerRenew(int id)
        {
            if (!IsCustomer())
            {
                return RedirectToAction("Index", "Home");
            }

            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToAction("Login", "Auth");
            }

            int userId = int.Parse(userIdStr);

            var ticket = _context.MonthlyTickets
                .Include(t => t.Vehicle)
                .FirstOrDefault(t => t.Id == id
                                  && t.Vehicle != null
                                  && t.Vehicle.UserId == userId);

            if (ticket == null)
            {
                TempData["Error"] = "Không tìm thấy vé tháng hoặc bạn không có quyền gia hạn vé này.";
                return RedirectToAction("CustomerRenewList");
            }

            return View(ticket);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CustomerRenew(int id, DateTime newEndDate)
        {
            if (!IsCustomer())
            {
                return RedirectToAction("Index", "Home");
            }

            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToAction("Login", "Auth");
            }

            int userId = int.Parse(userIdStr);

            var ticket = _context.MonthlyTickets
                .Include(t => t.Vehicle)
                .FirstOrDefault(t => t.Id == id
                                  && t.Vehicle != null
                                  && t.Vehicle.UserId == userId);

            if (ticket == null)
            {
                TempData["Error"] = "Không tìm thấy vé tháng hoặc bạn không có quyền gia hạn vé này.";
                return RedirectToAction("CustomerRenewList");
            }

            if (newEndDate <= ticket.EndDate)
            {
                TempData["Error"] = "Ngày gia hạn mới phải lớn hơn ngày hết hạn hiện tại.";
                return RedirectToAction("CustomerRenew", new { id });
            }

            ticket.Price += CalculateMonthlyPrice(ticket.EndDate.AddDays(1), newEndDate);
            ticket.EndDate = newEndDate;
            ticket.Status = "ConHan";

            _context.SaveChanges();

            LogActivity("Khách hàng gia hạn vé tháng",
                $"Khách hàng gia hạn vé tháng ID {ticket.Id} đến ngày {ticket.EndDate:dd/MM/yyyy}.");

            TempData["Success"] = "Gia hạn vé tháng thành công.";
            return RedirectToAction("CustomerRenewList");
        }
        [HttpGet]
        public IActionResult Create()
        {
            if (!IsStaffOrManager())
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Vehicles = _context.Vehicles.OrderBy(v => v.LicensePlate).ToList();
            return View();
        }

        [HttpPost]
        public IActionResult Create(MonthlyTicket ticket)
        {
            if (!IsStaffOrManager())
            {
                return RedirectToAction("Index", "Home");
            }

            if (ticket.EndDate <= ticket.StartDate)
            {
                ModelState.AddModelError("", "Ngày hết hạn phải lớn hơn ngày bắt đầu.");
            }

            var activeTicket = _context.MonthlyTickets
                .FirstOrDefault(t => t.VehicleId == ticket.VehicleId && t.Status == "ConHan");

            if (activeTicket != null)
            {
                ModelState.AddModelError("", "Xe này đã có vé tháng còn hạn.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Vehicles = _context.Vehicles.OrderBy(v => v.LicensePlate).ToList();
                return View(ticket);
            }

            ticket.Price = CalculateMonthlyPrice(ticket.StartDate, ticket.EndDate);
            ticket.Status = ticket.EndDate >= DateTime.Today ? "ConHan" : "HetHan";

            _context.MonthlyTickets.Add(ticket);
            _context.SaveChanges();
            LogActivity("Tạo vé tháng", $"Tạo vé tháng cho xe ID {ticket.VehicleId}, chủ vé {ticket.OwnerName}.");

            TempData["Success"] = "Tạo vé tháng thành công.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Renew(int id)
        {
            if (!IsStaffOrManager())
            {
                return RedirectToAction("Index", "Home");
            }

            var ticket = _context.MonthlyTickets
                .Include(t => t.Vehicle)
                .FirstOrDefault(t => t.Id == id);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        [HttpPost]
        public IActionResult Renew(int id, DateTime newEndDate)
        {
            if (!IsStaffOrManager())
            {
                return RedirectToAction("Index", "Home");
            }

            var ticket = _context.MonthlyTickets
                .Include(t => t.Vehicle)
                .FirstOrDefault(t => t.Id == id);

            if (ticket == null)
            {
                return NotFound();
            }

            if (newEndDate <= ticket.EndDate)
            {
                TempData["Error"] = "Ngày gia hạn mới phải lớn hơn ngày hết hạn hiện tại.";
                return RedirectToAction("Renew", new { id });
            }

            ticket.Price += CalculateMonthlyPrice(ticket.EndDate.AddDays(1), newEndDate);
            ticket.EndDate = newEndDate;
            ticket.Status = "ConHan";

            _context.SaveChanges();
            LogActivity("Gia hạn vé tháng", $"Gia hạn vé tháng ID {ticket.Id} đến ngày {ticket.EndDate:dd/MM/yyyy}.");

            TempData["Success"] = "Gia hạn vé tháng thành công.";
            return RedirectToAction("Index");
        }

        public IActionResult ExpiringSoon()
        {
            if (!IsStaffOrManager())
            {
                return RedirectToAction("Index", "Home");
            }

            UpdateTicketStatus();

            var today = DateTime.Today;
            var next7Days = today.AddDays(7);

            var tickets = _context.MonthlyTickets
                .Include(t => t.Vehicle)
                .Where(t => t.EndDate >= today && t.EndDate <= next7Days)
                .OrderBy(t => t.EndDate)
                .ToList();

            return View(tickets);
        }

        private decimal CalculateMonthlyPrice(DateTime startDate, DateTime endDate)
        {
            var totalDays = (endDate - startDate).Days + 1;

            if (totalDays <= 31)
                return 100000;

            if (totalDays <= 62)
                return 200000;

            return 300000;
        }

        private void UpdateTicketStatus()
        {
            var tickets = _context.MonthlyTickets.ToList();

            foreach (var ticket in tickets)
            {
                ticket.Status = ticket.EndDate >= DateTime.Today ? "ConHan" : "HetHan";
            }

            _context.SaveChanges();
        }
    }
}
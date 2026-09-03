using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiXe.Data;
using QuanLyBaiXe.Models;

namespace QuanLyBaiXe.Controllers
{
    public class ParkingController : BaseController
    {
        private readonly AppDbContext _context;

        public ParkingController(AppDbContext context) : base(context)
        {
            _context = context;
        }

        private bool IsStaffOrManager()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "NhanVien" || role == "QuanLy";
        }

        public IActionResult Index()
        {
            if (!IsStaffOrManager())
            {
                return RedirectToAction("Index", "Home");
            }

            var sessions = _context.ParkingSessions
                .Include(p => p.Vehicle)
                .OrderByDescending(p => p.CheckInTime)
                .ToList();

            return View(sessions);
        }
        private bool IsCustomer()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "KhachHang";
        }
        [HttpGet]
        public IActionResult CustomerCheckIn()
        {
            if (!IsCustomer())
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }
        [HttpPost]
        public IActionResult CustomerCheckIn(Vehicle vehicle)
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
            vehicle.UserId = userId;

            if (!ModelState.IsValid)
            {
                return View(vehicle);
            }

            var existingVehicle = _context.Vehicles
                .FirstOrDefault(v => v.LicensePlate == vehicle.LicensePlate);

            if (existingVehicle == null)
            {
                _context.Vehicles.Add(vehicle);
                _context.SaveChanges();
                existingVehicle = vehicle;
            }
            else
            {
                existingVehicle.VehicleType = vehicle.VehicleType;
                existingVehicle.OwnerName = vehicle.OwnerName;
                existingVehicle.PhoneNumber = vehicle.PhoneNumber;
                existingVehicle.UserId = userId;
                _context.SaveChanges();
            }

            var activeSession = _context.ParkingSessions
                .FirstOrDefault(p => p.VehicleId == existingVehicle.Id && p.Status == "DangGui");

            if (activeSession != null)
            {
                ViewBag.Error = "Xe này đang ở trong bãi, không thể gửi lại.";
                return View(vehicle);
            }

            var session = new ParkingSession
            {
                VehicleId = existingVehicle.Id,
                CheckInTime = DateTime.Now,
                Status = "DangGui"
            };

            _context.ParkingSessions.Add(session);
            _context.SaveChanges();

            LogActivity("Khách hàng gửi xe", $"Khách hàng gửi xe biển số {existingVehicle.LicensePlate} vào bãi.");

            TempData["Success"] = "Gửi xe thành công.";
            return RedirectToAction("CustomerReceive");
        }
        [HttpGet]
        [HttpGet]
        public IActionResult CustomerReceive()
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

            var sessions = _context.ParkingSessions
                .Include(p => p.Vehicle)
                .Where(p => p.Status == "DangGui"
                            && p.Vehicle != null
                            && p.Vehicle.UserId == userId)
                .OrderByDescending(p => p.CheckInTime)
                .ToList();

            return View(sessions);
        }
        [HttpPost]
        public IActionResult CustomerCheckOut(int id)
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

            var session = _context.ParkingSessions
                .Include(p => p.Vehicle)
                .FirstOrDefault(p => p.Id == id
                                     && p.Status == "DangGui"
                                     && p.Vehicle != null
                                     && p.Vehicle.UserId == userId);

            if (session == null)
            {
                TempData["Error"] = "Không tìm thấy xe đang gửi.";
                return RedirectToAction("CustomerReceive");
            }

            session.CheckOutTime = DateTime.Now;
            session.Fee = CalculateFee(
                session.CheckInTime,
                session.CheckOutTime.Value,
                session.Vehicle?.VehicleType ?? ""
            );
            session.Status = "DaLay";

            _context.SaveChanges();

            LogActivity("Khách hàng nhận xe", $"Khách hàng nhận xe biển số {session.Vehicle?.LicensePlate}, phí {session.Fee:N0} đ.");

            return View("CustomerCheckOutResult", session);
        }
        [HttpGet]
        public IActionResult CheckIn()
        {
            if (!IsStaffOrManager())
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }
        [HttpPost]
        public IActionResult CheckIn(Vehicle vehicle)
        {
            if (!IsStaffOrManager())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View(vehicle);
            }

            var existingVehicle = _context.Vehicles
                .FirstOrDefault(v => v.LicensePlate == vehicle.LicensePlate);

            if (existingVehicle == null)
            {
                _context.Vehicles.Add(vehicle);
                _context.SaveChanges();
                existingVehicle = vehicle;
            }
            else
            {
                existingVehicle.VehicleType = vehicle.VehicleType;
                existingVehicle.OwnerName = vehicle.OwnerName;
                existingVehicle.PhoneNumber = vehicle.PhoneNumber;
                _context.SaveChanges();
            }

            var activeSession = _context.ParkingSessions
                .FirstOrDefault(p => p.VehicleId == existingVehicle.Id && p.Status == "DangGui");

            if (activeSession != null)
            {
                ViewBag.Error = "Xe này đang ở trong bãi, không thể gửi lại.";
                return View(vehicle);
            }

            var session = new ParkingSession
            {
                VehicleId = existingVehicle.Id,
                CheckInTime = DateTime.Now,
                Status = "DangGui"
            };

            _context.ParkingSessions.Add(session);
            _context.SaveChanges();

            LogActivity("Nhập xe vào", $"Nhập xe biển số {existingVehicle.LicensePlate} vào bãi.");

            TempData["Success"] = "Nhập xe vào bãi thành công.";
            return RedirectToAction("CurrentVehicles");
        }

        public IActionResult CurrentVehicles()
        {
            if (!IsStaffOrManager())
            {
                return RedirectToAction("Index", "Home");
            }

            var currentVehicles = _context.ParkingSessions
                .Include(p => p.Vehicle)
                .Where(p => p.Status == "DangGui")
                .OrderByDescending(p => p.CheckInTime)
                .ToList();

            return View(currentVehicles);
        }

        [HttpGet]
        public IActionResult Search(string keyword)
        {
            if (!IsStaffOrManager())
            {
                return RedirectToAction("Index", "Home");
            }

            var query = _context.ParkingSessions
                .Include(p => p.Vehicle)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.Trim().ToLower();

                query = query.Where(p =>
                    p.Vehicle != null &&
                    (
                        p.Vehicle.LicensePlate.ToLower().Contains(keyword) ||
                        (p.Vehicle.OwnerName != null && p.Vehicle.OwnerName.ToLower().Contains(keyword)) ||
                        (p.Vehicle.PhoneNumber != null && p.Vehicle.PhoneNumber.Contains(keyword))
                    ));
            }

            var result = query
                .OrderByDescending(p => p.CheckInTime)
                .ToList();

            ViewBag.Keyword = keyword;
            return View(result);
        }

        [HttpGet]
        public IActionResult LostTicket()
        {
            if (!IsStaffOrManager())
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        public IActionResult LostTicket(string licensePlate, string? note)
        {
            if (!IsStaffOrManager())
            {
                return RedirectToAction("Index", "Home");
            }

            if (string.IsNullOrWhiteSpace(licensePlate))
            {
                ViewBag.Error = "Vui lòng nhập biển số xe.";
                return View();
            }

            var session = _context.ParkingSessions
                .Include(p => p.Vehicle)
                .FirstOrDefault(p => p.Vehicle != null
                                     && p.Vehicle.LicensePlate == licensePlate
                                     && p.Status == "DangGui");

            if (session == null)
            {
                ViewBag.Error = "Không tìm thấy xe đang gửi với biển số này.";
                return View();
            }

            session.CheckOutTime = DateTime.Now;
            session.IsLostTicket = true;
            session.Note = note;
            session.Fee = CalculateLostTicketFee(
                session.CheckInTime,
                session.CheckOutTime.Value,
                session.Vehicle?.VehicleType ?? ""
            );
            session.Status = "MatVe_DaLay";

            _context.SaveChanges();

            LogActivity("Xử lý mất vé", $"Xử lý mất vé cho xe biển số {session.Vehicle?.LicensePlate}, phí {session.Fee:N0} đ.");

            return RedirectToAction("LostTicketResult", new { id = session.Id });
        }

        [HttpGet]
        public IActionResult LostTicketResult(int id)
        {
            if (!IsStaffOrManager())
            {
                return RedirectToAction("Index", "Home");
            }

            var session = _context.ParkingSessions
                .Include(p => p.Vehicle)
                .FirstOrDefault(p => p.Id == id);

            if (session == null)
            {
                return NotFound();
            }

            return View(session);
        }

        [HttpGet]
        public IActionResult CheckOut(int id)
        {
            if (!IsStaffOrManager())
            {
                return RedirectToAction("Index", "Home");
            }

            var session = _context.ParkingSessions
                .Include(p => p.Vehicle)
                .FirstOrDefault(p => p.Id == id);

            if (session == null)
            {
                return NotFound();
            }

            if (session.Status != "DangGui")
            {
                TempData["Error"] = "Phiên gửi xe này đã được xử lý xe ra.";
                return RedirectToAction("CurrentVehicles");
            }

            session.CheckOutTime = DateTime.Now;
            session.Fee = CalculateFee(
                session.CheckInTime,
                session.CheckOutTime.Value,
                session.Vehicle?.VehicleType ?? ""
            );
            session.Status = "DaLay";

            _context.SaveChanges();

            LogActivity("Xử lý xe ra", $"Xử lý xe ra cho biển số {session.Vehicle?.LicensePlate}, phí {session.Fee:N0} đ.");

            return View(session);
        }

        private decimal CalculateFee(DateTime checkIn, DateTime checkOut, string vehicleType)
        {
            var hours = (checkOut - checkIn).TotalHours;

            if (vehicleType == "Xe máy")
            {
                if (hours <= 12)
                    return 5000;
                return 10000;
            }

            if (vehicleType == "Ô tô")
            {
                if (hours <= 12)
                    return 30000;
                return 50000;
            }

            return 0;
        }

        private decimal CalculateLostTicketFee(DateTime checkIn, DateTime checkOut, string vehicleType)
        {
            decimal normalFee = CalculateFee(checkIn, checkOut, vehicleType);

            if (vehicleType == "Xe máy")
            {
                return normalFee + 50000;
            }

            if (vehicleType == "Ô tô")
            {
                return normalFee + 200000;
            }

            return normalFee;
        }
    }
}
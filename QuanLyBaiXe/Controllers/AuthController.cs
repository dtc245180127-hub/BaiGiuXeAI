using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiXe.Data;
using QuanLyBaiXe.ViewModels;

namespace QuanLyBaiXe.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("Username") != null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _context.Users
                .Include(u => u.Role)
                .FirstOrDefault(u =>
                    u.Username == model.Username &&
                    u.Password == model.Password);

            if (user == null)
            {
                ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng";
                return View(model);
            }

            // Tạo một mã phiên chatbot mới cho mỗi lần đăng nhập
            var chatSessionId = Guid.NewGuid().ToString();

            HttpContext.Session.SetString(
                "UserId",
                user.Id.ToString());

            HttpContext.Session.SetString(
                "Username",
                user.Username);

            HttpContext.Session.SetString(
                "FullName",
                user.FullName);

            HttpContext.Session.SetString(
                "Role",
                user.Role?.RoleName ?? "");

            HttpContext.Session.SetString(
                "ChatSessionId",
                chatSessionId);

            _context.ActivityLogs.Add(
                new QuanLyBaiXe.Models.ActivityLog
                {
                    UserFullName = user.FullName,
                    Username = user.Username,
                    Role = user.Role?.RoleName ?? "",
                    ActionName = "Đăng nhập",
                    Description =
                        $"Người dùng {user.Username} đã đăng nhập vào hệ thống.",
                    CreatedAt = DateTime.Now
                });

            _context.SaveChanges();

            var role = user.Role?.RoleName ?? "";

            if (role == "QuanLy")
            {
                return RedirectToAction(
                    "AdminDashboard",
                    "Home");
            }
            else if (role == "NhanVien")
            {
                return RedirectToAction(
                    "StaffDashboard",
                    "Home");
            }
            else
            {
                return RedirectToAction(
                    "CustomerDashboard",
                    "Home");
            }
        }

        public IActionResult Logout()
        {
            var fullName =
                HttpContext.Session.GetString("FullName") ?? "";

            var username =
                HttpContext.Session.GetString("Username") ?? "";

            var role =
                HttpContext.Session.GetString("Role") ?? "";

            _context.ActivityLogs.Add(
                new QuanLyBaiXe.Models.ActivityLog
                {
                    UserFullName = fullName,
                    Username = username,
                    Role = role,
                    ActionName = "Đăng xuất",
                    Description =
                        $"Người dùng {username} đã đăng xuất khỏi hệ thống.",
                    CreatedAt = DateTime.Now
                });

            _context.SaveChanges();

            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }
    }
}
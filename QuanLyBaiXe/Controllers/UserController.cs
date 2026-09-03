using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiXe.Data;
using QuanLyBaiXe.Models;

namespace QuanLyBaiXe.Controllers
{
    public class UserController : BaseController
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context) : base(context)
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

            var users = _context.Users
                .Include(u => u.Role)
                .OrderBy(u => u.Id)
                .ToList();

            return View(users);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsManager())
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Roles = _context.Roles.OrderBy(r => r.Id).ToList();
            return View();
        }

        [HttpPost]
        public IActionResult Create(User user)
        {
            if (!IsManager())
            {
                return RedirectToAction("Index", "Home");
            }

            var existedUser = _context.Users.FirstOrDefault(u => u.Username == user.Username);
            if (existedUser != null)
            {
                ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = _context.Roles.OrderBy(r => r.Id).ToList();
                return View(user);
            }

            _context.Users.Add(user);
            _context.SaveChanges();
            LogActivity("Thêm tài khoản", $"Thêm tài khoản {user.Username} với vai trò ID {user.RoleId}.");

            TempData["Success"] = "Thêm tài khoản thành công.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!IsManager())
            {
                return RedirectToAction("Index", "Home");
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            ViewBag.Roles = _context.Roles.OrderBy(r => r.Id).ToList();
            return View(user);
        }

        [HttpPost]
        public IActionResult Edit(User user)
        {
            if (!IsManager())
            {
                return RedirectToAction("Index", "Home");
            }

            var existedUser = _context.Users.FirstOrDefault(u => u.Username == user.Username && u.Id != user.Id);
            if (existedUser != null)
            {
                ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = _context.Roles.OrderBy(r => r.Id).ToList();
                return View(user);
            }

            var currentUser = _context.Users.FirstOrDefault(u => u.Id == user.Id);
            if (currentUser == null)
            {
                return NotFound();
            }

            currentUser.FullName = user.FullName;
            currentUser.Username = user.Username;
            currentUser.Password = user.Password;
            currentUser.RoleId = user.RoleId;

            _context.SaveChanges();
            LogActivity("Sửa tài khoản", $"Cập nhật tài khoản {currentUser.Username}.");

            TempData["Success"] = "Cập nhật tài khoản thành công.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            if (!IsManager())
            {
                return RedirectToAction("Index", "Home");
            }

            var user = _context.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            if (!IsManager())
            {
                return RedirectToAction("Index", "Home");
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            var currentSessionUserId = HttpContext.Session.GetString("UserId");
            if (currentSessionUserId == user.Id.ToString())
            {
                TempData["Error"] = "Bạn không thể xóa chính tài khoản đang đăng nhập.";
                return RedirectToAction("Index");
            }

            _context.Users.Remove(user);
            LogActivity("Xóa tài khoản", $"Xóa tài khoản {user.Username}.");
            _context.SaveChanges();

            TempData["Success"] = "Xóa tài khoản thành công.";
            return RedirectToAction("Index");
        }
    }
}
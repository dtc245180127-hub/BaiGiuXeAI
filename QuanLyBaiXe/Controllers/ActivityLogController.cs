using Microsoft.AspNetCore.Mvc;
using QuanLyBaiXe.Data;

namespace QuanLyBaiXe.Controllers
{
    public class ActivityLogController : BaseController
    {
        private readonly AppDbContext _context;

        public ActivityLogController(AppDbContext context) : base(context)
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

            var logs = _context.ActivityLogs
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            return View(logs);
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using QuanLyBaiXe.Data;
using QuanLyBaiXe.Models;

namespace QuanLyBaiXe.Controllers
{
    public class BaseController : Controller
    {
        protected readonly AppDbContext? _context;

        public BaseController()
        {
        }

        public BaseController(AppDbContext context)
        {
            _context = context;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(username))
            {
                if (context.RouteData.Values["controller"]?.ToString() != "Auth")
                {
                    context.Result = RedirectToAction("Login", "Auth");
                }
            }

            base.OnActionExecuting(context);
        }

        protected void LogActivity(string actionName, string description)
        {
            if (_context == null) return;

            var log = new ActivityLog
            {
                UserFullName = HttpContext.Session.GetString("FullName") ?? "",
                Username = HttpContext.Session.GetString("Username") ?? "",
                Role = HttpContext.Session.GetString("Role") ?? "",
                ActionName = actionName,
                Description = description,
                CreatedAt = DateTime.Now
            };

            _context.ActivityLogs.Add(log);
            _context.SaveChanges();
        }
    }
}
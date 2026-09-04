using Microsoft.EntityFrameworkCore;
using QuanLyBaiXe.Data;
using QuanLyBaiXe.Services;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký MVC
builder.Services.AddControllersWithViews();

// Đăng ký HttpClient để gọi Gemini API
builder.Services.AddHttpClient();

// Đăng ký các dịch vụ AI
builder.Services.AddScoped<AIAnalysisService>();
builder.Services.AddScoped<AIStaffingService>();

// Đăng ký kết nối cơ sở dữ liệu
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Đăng ký Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Cấu hình xử lý lỗi
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Chuyển hướng HTTP sang HTTPS
app.UseHttpsRedirection();

// Cho phép sử dụng file tĩnh trong wwwroot
app.UseStaticFiles();

// Cấu hình routing
app.UseRouting();

// Kích hoạt Session
app.UseSession();

// Phân quyền
app.UseAuthorization();

// Route mặc định
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
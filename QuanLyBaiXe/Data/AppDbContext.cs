using Microsoft.EntityFrameworkCore;
using QuanLyBaiXe.Models;

namespace QuanLyBaiXe.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<ParkingSession> ParkingSessions { get; set; }
        public DbSet<MonthlyTicket> MonthlyTickets { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, RoleName = "QuanLy" },
                new Role { Id = 2, RoleName = "NhanVien" },
                new Role { Id = 3, RoleName = "KhachHang" }
            );

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    FullName = "Quản lý hệ thống",
                    Username = "admin",
                    Password = "123456",
                    RoleId = 1
                },
                new User
                {
                    Id = 2,
                    FullName = "Nhân viên bãi xe",
                    Username = "nhanvien",
                    Password = "123456",
                    RoleId = 2
                },
                new User
                {
                    Id = 3,
                    FullName = "Khách hàng demo",
                    Username = "khachhang",
                    Password = "123456",
                    RoleId = 3
                }
            );
        }
    }
}
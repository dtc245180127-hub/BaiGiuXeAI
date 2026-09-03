using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyBaiXe.Models
{
    public class Vehicle
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập biển số xe")]
        [StringLength(20)]
        public string LicensePlate { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn loại xe")]
        [StringLength(30)]
        public string VehicleType { get; set; } = string.Empty;

        [StringLength(100)]
        public string? OwnerName { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        public int? UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        public ICollection<ParkingSession>? ParkingSessions { get; set; }
    }
}
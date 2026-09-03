using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyBaiXe.Models
{
    public class ParkingSession
    {
        public int Id { get; set; }

        [Required]
        public int VehicleId { get; set; }

        [ForeignKey("VehicleId")]
        public Vehicle? Vehicle { get; set; }

        [Required]
        public DateTime CheckInTime { get; set; }

        public DateTime? CheckOutTime { get; set; }

        public decimal? Fee { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "DangGui";

        public bool IsLostTicket { get; set; } = false;

        [StringLength(255)]
        public string? Note { get; set; }
    }
}
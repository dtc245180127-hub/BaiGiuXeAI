using System.ComponentModel.DataAnnotations;

namespace QuanLyBaiXe.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }

        [StringLength(100)]
        public string UserFullName { get; set; } = string.Empty;

        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [StringLength(50)]
        public string Role { get; set; } = string.Empty;

        [StringLength(100)]
        public string ActionName { get; set; } = string.Empty;

        [StringLength(255)]
        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
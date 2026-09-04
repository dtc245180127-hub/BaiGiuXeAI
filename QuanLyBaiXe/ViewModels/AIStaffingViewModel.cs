
namespace QuanLyBaiXe.ViewModels
{
    public class AIStaffingViewModel
    {
        public string Status { get; set; } = string.Empty;

        public string Summary { get; set; } = string.Empty;

        public List<string> PeakHours { get; set; } = new();

        public List<string> Recommendations { get; set; } = new();

        public List<string> Notes { get; set; } = new();

        public int AvailableStaff { get; set; }

        public int CurrentVehicles { get; set; }
    }
}
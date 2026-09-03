namespace QuanLyBaiXe.ViewModels
{
    public class ReportViewModel
    {
        public decimal TodayRevenue { get; set; }
        public decimal TotalRevenue { get; set; }

        public int TotalVehiclesInParking { get; set; }
        public int TotalCheckedOutVehicles { get; set; }
        public int TotalLostTicketVehicles { get; set; }
        public int TotalMonthlyTicketsActive { get; set; }

        public List<RevenueByDateViewModel> RevenueByDates { get; set; } = new List<RevenueByDateViewModel>();
    }

    public class RevenueByDateViewModel
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
    }
}
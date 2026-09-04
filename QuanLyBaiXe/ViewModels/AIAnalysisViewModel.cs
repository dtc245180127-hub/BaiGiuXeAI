namespace QuanLyBaiXe.ViewModels
{
    public class AIAnalysisViewModel
    {
        public string Status { get; set; } = string.Empty;

        public string Summary { get; set; } = string.Empty;

        public List<string> Insights { get; set; } = new();

        public List<string> Recommendations { get; set; } = new();
    }
}
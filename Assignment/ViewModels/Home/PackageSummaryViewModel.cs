namespace Assignment.ViewModels.Home
{
    public class PackageSummaryViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public List<string> Highlights { get; set; } = new();
    }
}


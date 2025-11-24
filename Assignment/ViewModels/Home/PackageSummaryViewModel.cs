namespace Assignment.ViewModels.Home
{
    public class PackageSummaryViewModel
    {
        public int PackageId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public string? ImageUrl { get; set; }
        public List<string> Highlights { get; set; } = new();
        public int? RoomTypeId { get; set; }
        public string? HotelName { get; set; }
        public string? Location { get; set; }
        public decimal? IndividualPrice { get; set; }
        public string? RoomTypeName { get; set; }
        public int? Occupancy { get; set; }
    }
}


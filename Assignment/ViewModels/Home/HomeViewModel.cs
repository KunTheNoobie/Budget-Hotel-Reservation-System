using Assignment.Models;
using System.Collections.Generic;

namespace Assignment.ViewModels.Home
{
    public class HomeViewModel
    {
        public List<RoomType> FeaturedRooms { get; set; } = new();
        public Dictionary<int, RoomReviewInfo> ReviewData { get; set; } = new();
        public List<string> Destinations { get; set; } = new();
        public HomeStatsViewModel Stats { get; set; } = new();
        public List<PackageSummaryViewModel> Packages { get; set; } = new();
        public List<Service> HighlightedServices { get; set; } = new();
    }
}


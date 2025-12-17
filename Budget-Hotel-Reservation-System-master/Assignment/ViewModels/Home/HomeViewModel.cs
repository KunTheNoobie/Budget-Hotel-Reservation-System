using Assignment.Models;
using System.Collections.Generic;

namespace Assignment.ViewModels.Home
{
    /// <summary>
    /// View model for the home page, containing all data needed to display
    /// featured rooms, packages, statistics, reviews, and other home page content.
    /// </summary>
    public class HomeViewModel
    {
        public List<RoomType> FeaturedRooms { get; set; } = new();
        public Dictionary<int, RoomReviewInfo> ReviewData { get; set; } = new();
        public List<string> Destinations { get; set; } = new();
        public HomeStatsViewModel Stats { get; set; } = new();
        public List<PackageSummaryViewModel> Packages { get; set; } = new();
        public List<Service> HighlightedServices { get; set; } = new();
        public List<Review> CustomerReviews { get; set; } = new();
    }
}


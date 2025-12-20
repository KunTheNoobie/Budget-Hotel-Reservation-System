using Assignment.Models;
using System.Collections.Generic;

namespace Assignment.ViewModels.Home
{
    /// <summary>
    /// View model for the home page, containing all data needed to display
    /// featured rooms, packages, statistics, reviews, and other home page content.
    /// This view model aggregates data from multiple sources to populate the home page.
    /// </summary>
    public class HomeViewModel
    {
        /// <summary>
        /// List of featured room types to display on the home page.
        /// Typically shows the most popular or highest-rated room types.
        /// Used to showcase available accommodations to potential customers.
        /// </summary>
        public List<RoomType> FeaturedRooms { get; set; } = new();
        
        /// <summary>
        /// Dictionary mapping room type IDs to their review information.
        /// Key: RoomTypeId (int)
        /// Value: RoomReviewInfo (contains average rating, review count, etc.)
        /// Used to display review statistics for each featured room type.
        /// </summary>
        public Dictionary<int, RoomReviewInfo> ReviewData { get; set; } = new();
        
        /// <summary>
        /// List of destination city names where hotels are located.
        /// Used to populate destination dropdowns or display popular destinations.
        /// Example: ["Kuala Lumpur", "George Town", "Malacca", "Ipoh"]
        /// </summary>
        public List<string> Destinations { get; set; } = new();
        
        /// <summary>
        /// Statistics about the hotel reservation system.
        /// Contains counts of hotels, rooms, bookings, customers, etc.
        /// Used to display system statistics on the home page (e.g., "500+ Hotels", "10,000+ Bookings").
        /// </summary>
        public HomeStatsViewModel Stats { get; set; } = new();
        
        /// <summary>
        /// List of package summaries to display on the home page.
        /// Packages are special offers that combine rooms with services (e.g., "Romantic Getaway Package").
        /// Used to promote special deals and packages to customers.
        /// </summary>
        public List<PackageSummaryViewModel> Packages { get; set; } = new();
        
        /// <summary>
        /// List of highlighted services to display on the home page.
        /// Services are additional amenities offered by hotels (e.g., "Airport Transfer", "Breakfast Buffet").
        /// Used to showcase available services to potential customers.
        /// </summary>
        public List<Service> HighlightedServices { get; set; } = new();
        
        /// <summary>
        /// List of customer reviews to display on the home page.
        /// Typically shows recent reviews or highly-rated reviews.
        /// Used to build trust and showcase customer satisfaction.
        /// </summary>
        public List<Review> CustomerReviews { get; set; } = new();
    }
}


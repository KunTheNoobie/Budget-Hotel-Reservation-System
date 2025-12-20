namespace Assignment.ViewModels.Home
{
    /// <summary>
    /// View model for displaying package summaries on the home page.
    /// Contains package information, pricing, highlights, and associated room type details.
    /// Used to showcase package deals to customers on the home page.
    /// </summary>
    public class PackageSummaryViewModel
    {
        /// <summary>
        /// Unique identifier for the package.
        /// Used for navigation to package details and booking.
        /// </summary>
        public int PackageId { get; set; }
        
        /// <summary>
        /// Package name (e.g., "Kuala Lumpur City Explorer", "Honeymoon Bliss").
        /// Displayed as the package title on the home page.
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Package description explaining what's included.
        /// Displayed below the package name on the home page.
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Total price of the package (includes room + all services).
        /// This is the fixed price customers pay for the entire package.
        /// Promotions do NOT apply to packages (packages have fixed prices).
        /// </summary>
        public decimal TotalPrice { get; set; }
        
        /// <summary>
        /// URL to the package image for display on the home page.
        /// Optional - if null, a default image is used.
        /// </summary>
        public string? ImageUrl { get; set; }
        
        /// <summary>
        /// List of highlights showing what's included in the package.
        /// Format: "Service Name xQuantity" or "Room Type Name xQuantity".
        /// Example: ["Breakfast Buffet x2", "Airport Transfer x1", "Spa Treatment x1"]
        /// Displayed as bullet points on the home page.
        /// </summary>
        public List<string> Highlights { get; set; } = new();
        
        /// <summary>
        /// Room type ID associated with this package.
        /// Used to link to the room type for booking.
        /// Optional - packages may not always have a room type.
        /// </summary>
        public int? RoomTypeId { get; set; }
        
        /// <summary>
        /// Name of the hotel where this package is available.
        /// Optional - used for display purposes.
        /// </summary>
        public string? HotelName { get; set; }
        
        /// <summary>
        /// Location of the hotel (City, Country format).
        /// Optional - used for display purposes.
        /// </summary>
        public string? Location { get; set; }
        
        /// <summary>
        /// Individual price if items were purchased separately.
        /// Used to show savings (TotalPrice vs IndividualPrice).
        /// Optional - used for display purposes.
        /// </summary>
        public decimal? IndividualPrice { get; set; }
        
        /// <summary>
        /// Name of the room type included in the package.
        /// Optional - used for display purposes.
        /// </summary>
        public string? RoomTypeName { get; set; }
        
        /// <summary>
        /// Maximum occupancy of the room type in the package.
        /// Indicates how many guests can stay in the room.
        /// Optional - used for display purposes.
        /// </summary>
        public int? Occupancy { get; set; }
    }
}


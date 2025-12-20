namespace Assignment.ViewModels.Home
{
    /// <summary>
    /// View model for displaying review statistics for a room type.
    /// Contains the total number of reviews and average rating.
    /// Used on home page and room detail pages to show review summary.
    /// </summary>
    public class RoomReviewInfo
    {
        /// <summary>
        /// Total number of reviews submitted for this room type.
        /// Calculated from all bookings of all rooms of this type.
        /// </summary>
        public int ReviewCount { get; set; }
        
        /// <summary>
        /// Average rating (1-5 stars) across all reviews for this room type.
        /// Calculated as the mean of all review ratings.
        /// Defaults to 4.5 if no reviews exist (for display purposes).
        /// </summary>
        public double AverageRating { get; set; }
    }
}


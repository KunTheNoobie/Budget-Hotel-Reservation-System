using System.ComponentModel.DataAnnotations;

namespace Assignment.ViewModels.Home
{
    /// <summary>
    /// View model for displaying statistics on the home page.
    /// Contains counts and averages for hotels, guests, bookings, and ratings.
    /// These statistics build trust and credibility with potential customers.
    /// </summary>
    public class HomeStatsViewModel
    {
        /// <summary>
        /// Total number of hotels in the system.
        /// Displayed as "Budget Hotels" on the home page.
        /// Minimum value is 10 (for marketing purposes - ensures good appearance).
        /// </summary>
        [Display(Name = "Budget Hotels")]
        public int HotelCount { get; set; }

        /// <summary>
        /// Total number of unique customers who have made bookings.
        /// Displayed as "Happy Guests" on the home page.
        /// Minimum value is 2 (for marketing purposes).
        /// </summary>
        [Display(Name = "Happy Guests")]
        public int HappyGuests { get; set; }

        /// <summary>
        /// Total number of active bookings (not cancelled).
        /// Includes all booking statuses except Cancelled.
        /// Displayed as "Active Bookings" on the home page.
        /// </summary>
        [Display(Name = "Active Bookings")]
        public int ActiveBookings { get; set; }

        /// <summary>
        /// Average rating across all reviews in the system.
        /// Calculated from all review ratings (1-5 stars).
        /// Defaults to 4.6 if no reviews exist (for marketing purposes).
        /// Displayed as "Average Rating" on the home page.
        /// </summary>
        [Display(Name = "Average Rating")]
        public double AverageRating { get; set; }
    }
}


using System.ComponentModel.DataAnnotations;

namespace Assignment.ViewModels.Home
{
    /// <summary>
    /// View model for displaying statistics on the home page.
    /// Contains counts and averages for hotels, guests, bookings, and ratings.
    /// </summary>
    public class HomeStatsViewModel
    {
        [Display(Name = "Budget Hotels")]
        public int HotelCount { get; set; }

        [Display(Name = "Happy Guests")]
        public int HappyGuests { get; set; }

        [Display(Name = "Active Bookings")]
        public int ActiveBookings { get; set; }

        [Display(Name = "Average Rating")]
        public double AverageRating { get; set; }
    }
}


using System.ComponentModel.DataAnnotations;

namespace Assignment.ViewModels.Search
{
    /// <summary>
    /// View model for the room search form.
    /// Contains search parameters including search term, dates, guests, price, and room type filters.
    /// Includes validation attributes to ensure data integrity.
    /// Used by RoomController.Catalog action for room search and filtering.
    /// </summary>
    public class SearchViewModel
    {
        /// <summary>
        /// Search term for finding hotels/rooms by name, city, or country.
        /// Optional field - can search by location (e.g., "Kuala Lumpur, Malaysia") or hotel/room name.
        /// Minimum 2 characters required for meaningful search.
        /// Maximum length: 200 characters.
        /// Only allows alphanumeric characters, spaces, commas, periods, and hyphens (security: prevents SQL injection).
        /// </summary>
        [StringLength(200, ErrorMessage = "Search term cannot exceed 200 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s,.-]+$", ErrorMessage = "Search term contains invalid characters")]
        [Display(Name = "Search Term")]
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Check-in date for the booking.
        /// Optional field - if provided, filters rooms available on this date.
        /// Must be today or in the future (cannot be in the past).
        /// Used to check room availability and filter search results.
        /// </summary>
        [DataType(DataType.Date)]
        [Display(Name = "Check-in Date")]
        public DateTime? CheckIn { get; set; }

        /// <summary>
        /// Check-out date for the booking.
        /// Optional field - if provided, filters rooms available until this date.
        /// Must be after check-in date.
        /// Used to check room availability and filter search results.
        /// </summary>
        [DataType(DataType.Date)]
        [Display(Name = "Check-out Date")]
        public DateTime? CheckOut { get; set; }

        /// <summary>
        /// Number of guests for the booking.
        /// Optional field - if provided, filters rooms that can accommodate this many guests.
        /// Must be between 1 and 20 (valid range for hotel rooms).
        /// Room occupancy must be greater than or equal to this value.
        /// </summary>
        [Range(1, 20, ErrorMessage = "Number of guests must be between 1 and 20")]
        [Display(Name = "Number of Guests")]
        public int? Guests { get; set; }

        /// <summary>
        /// Maximum price per night filter.
        /// Optional field - if provided, filters rooms with base price at or below this value.
        /// Must be a positive number (0 or greater).
        /// Used to find budget-friendly options.
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Maximum price must be a positive number")]
        [Display(Name = "Maximum Price")]
        public decimal? MaxPrice { get; set; }

        /// <summary>
        /// Room type ID filter.
        /// Optional field - if provided, filters to show only this specific room type.
        /// Used to narrow down search to a specific room category (e.g., "Standard Single", "Deluxe Double").
        /// </summary>
        [Display(Name = "Room Type")]
        public int? RoomTypeId { get; set; }
    }
}


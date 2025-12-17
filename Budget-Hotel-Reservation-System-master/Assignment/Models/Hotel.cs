using System.ComponentModel.DataAnnotations;

namespace Assignment.Models
{
    /// <summary>
    /// Represents a hotel property in the reservation system.
    /// Contains location information, contact details, and descriptive information about the hotel.
    /// </summary>
    public class Hotel
    {
        /// <summary>
        /// Primary key identifier for the hotel.
        /// </summary>
        [Key]
        public int HotelId { get; set; }

        /// <summary>
        /// Name of the hotel.
        /// Required field with maximum length of 150 characters.
        /// </summary>
        [Required, StringLength(150)]
        public string Name { get; set; }

        /// <summary>
        /// Street address of the hotel.
        /// Required field with maximum length of 255 characters.
        /// </summary>
        [Required, StringLength(255)]
        public string Address { get; set; }

        /// <summary>
        /// City where the hotel is located.
        /// Required field with maximum length of 50 characters.
        /// </summary>
        [Required, StringLength(50)]
        public string City { get; set; }

        /// <summary>
        /// Postal/ZIP code of the hotel's location.
        /// Optional field with maximum length of 10 characters.
        /// </summary>
        [StringLength(10)]
        public string? PostalCode { get; set; }

        /// <summary>
        /// Country where the hotel is located.
        /// Required field with maximum length of 50 characters.
        /// </summary>
        [Required, StringLength(50)]
        public string Country { get; set; }

        /// <summary>
        /// Contact phone number for the hotel.
        /// Optional field, must be a valid phone format, maximum length of 20 characters.
        /// </summary>
        [Phone, StringLength(20)]
        public string? ContactNumber { get; set; }

        /// <summary>
        /// Contact email address for the hotel.
        /// Optional field, must be a valid email format, maximum length of 100 characters.
        /// </summary>
        [EmailAddress, StringLength(100)]
        public string? ContactEmail { get; set; }

        /// <summary>
        /// Detailed description of the hotel, its amenities, and features.
        /// Optional field with maximum length of 1000 characters.
        /// </summary>
        [StringLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// URL to an image representing the hotel (exterior photo, logo, etc.).
        /// Optional field with maximum length of 255 characters.
        /// </summary>
        [StringLength(255)]
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Soft delete flag - indicates if the hotel has been deleted.
        /// When true, the hotel is hidden from queries but not physically removed from the database.
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Timestamp of when the hotel was soft deleted.
        /// Null if the hotel has not been deleted.
        /// </summary>
        public DateTime? DeletedAt { get; set; }
    }
}
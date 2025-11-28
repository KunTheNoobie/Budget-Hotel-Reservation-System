using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    /// <summary>
    /// Represents a review/rating submitted by a user for a completed booking.
    /// Allows customers to rate their stay experience and provide feedback.
    /// Reviews are linked to specific bookings to ensure only customers who have stayed can review.
    /// </summary>
    public class Review
    {
        /// <summary>
        /// Primary key identifier for the review.
        /// </summary>
        [Key]
        public int ReviewId { get; set; }

        /// <summary>
        /// Foreign key to the Booking that this review is for.
        /// A review must be associated with a specific booking.
        /// User information can be obtained from Booking.UserId.
        /// </summary>
        public int BookingId { get; set; }

        /// <summary>
        /// Navigation property - the booking that this review is for.
        /// User information can be accessed via Booking.User.
        /// </summary>
        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; }

        /// <summary>
        /// Rating given by the user, on a scale of 1 to 5 stars.
        /// 1 = Poor, 2 = Fair, 3 = Good, 4 = Very Good, 5 = Excellent.
        /// </summary>
        [Range(1, 5)]
        public int Rating { get; set; }

        /// <summary>
        /// Optional written comment/feedback provided by the user.
        /// Maximum length of 500 characters.
        /// </summary>
        [StringLength(500)]
        public string? Comment { get; set; }

        /// <summary>
        /// Date and time when the review was submitted.
        /// Defaults to the current date/time.
        /// </summary>
        public DateTime ReviewDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Soft delete flag - indicates if the review has been deleted.
        /// When true, the review is hidden from queries but not physically removed from the database.
        /// Used for review moderation (admin can hide inappropriate reviews).
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Timestamp of when the review was soft deleted.
        /// Null if the review has not been deleted.
        /// </summary>
        public DateTime? DeletedAt { get; set; }
    }
}
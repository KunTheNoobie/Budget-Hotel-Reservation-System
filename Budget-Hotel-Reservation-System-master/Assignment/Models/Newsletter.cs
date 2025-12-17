using System.ComponentModel.DataAnnotations;

namespace Assignment.Models
{
    /// <summary>
    /// Represents a newsletter subscription entry.
    /// Stores email addresses of users who have subscribed to receive promotional emails,
    /// newsletters, and marketing communications from the hotel.
    /// </summary>
    public class Newsletter
    {
        /// <summary>
        /// Primary key identifier for the newsletter subscription.
        /// </summary>
        [Key]
        public int NewsletterId { get; set; }

        /// <summary>
        /// Email address of the subscriber.
        /// Required field, must be a valid email format, maximum length of 100 characters.
        /// </summary>
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        /// <summary>
        /// Date and time when the user subscribed to the newsletter.
        /// Defaults to the current date/time.
        /// </summary>
        public DateTime SubscribedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Flag indicating whether the subscription is currently active.
        /// Can be set to false if the user unsubscribes, without deleting the record.
        /// Defaults to true when a new subscription is created.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Soft delete flag - indicates if the newsletter subscription has been deleted.
        /// When true, the subscription is hidden from queries but not physically removed from the database.
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Timestamp of when the newsletter subscription was soft deleted.
        /// Null if the subscription has not been deleted.
        /// </summary>
        public DateTime? DeletedAt { get; set; }
    }
}


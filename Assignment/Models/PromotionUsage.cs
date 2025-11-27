using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    /// <summary>
    /// Represents a record of when a promotion code was used in a booking.
    /// Tracks usage information for abuse prevention and validation purposes.
    /// Stores encrypted/hashed identifiers (phone number, card number) to prevent abuse
    /// while maintaining privacy and security.
    /// </summary>
    public class PromotionUsage
    {
        /// <summary>
        /// Primary key identifier for the promotion usage record.
        /// </summary>
        [Key]
        public int PromotionUsageId { get; set; }

        /// <summary>
        /// Foreign key to the Promotion that was used.
        /// </summary>
        [Required]
        public int PromotionId { get; set; }

        /// <summary>
        /// Navigation property - the promotion that was used.
        /// </summary>
        [ForeignKey("PromotionId")]
        public virtual Promotion? Promotion { get; set; }

        /// <summary>
        /// Foreign key to the Booking where this promotion was applied.
        /// </summary>
        [Required]
        public int BookingId { get; set; }

        /// <summary>
        /// Navigation property - the booking where this promotion was used.
        /// </summary>
        [ForeignKey("BookingId")]
        public virtual Booking? Booking { get; set; }

        // ========== Abuse Prevention Tracking (Encrypted/Hashed) ==========

        /// <summary>
        /// Encrypted hash of the phone number used in the booking.
        /// Used to enforce "LimitPerPhoneNumber" restrictions.
        /// Stored as encrypted string (not plain text) for privacy and security.
        /// Maximum length of 255 characters.
        /// </summary>
        [StringLength(255)]
        public string? PhoneNumberHash { get; set; }

        /// <summary>
        /// Identifier for the payment card used (last 4 digits + hash of full card number).
        /// Used to enforce "LimitPerPaymentCard" restrictions.
        /// Format: "XXXX-HASH" where XXXX is last 4 digits and HASH is encrypted identifier.
        /// Maximum length of 100 characters.
        /// </summary>
        [StringLength(100)]
        public string? CardIdentifier { get; set; }

        /// <summary>
        /// Device fingerprint or identifier from the client device/browser.
        /// Used to enforce "LimitPerDevice" restrictions.
        /// Maximum length of 100 characters.
        /// </summary>
        [StringLength(100)]
        public string? DeviceFingerprint { get; set; }

        /// <summary>
        /// IP address from which the booking was made.
        /// Used to enforce "LimitPerDevice" restrictions and for security monitoring.
        /// Maximum length of 50 characters (supports IPv6).
        /// </summary>
        [StringLength(50)]
        public string? IpAddress { get; set; }

        /// <summary>
        /// Date and time when the promotion was used/applied to the booking.
        /// Defaults to the current date/time.
        /// </summary>
        public DateTime UsedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Foreign key to the User who used this promotion.
        /// Used to enforce "LimitPerUserAccount" restrictions.
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Navigation property - the user who used this promotion.
        /// </summary>
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        /// <summary>
        /// Soft delete flag - indicates if the promotion usage record has been deleted.
        /// When true, the record is hidden from queries but not physically removed from the database.
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Timestamp of when the promotion usage record was soft deleted.
        /// Null if the record has not been deleted.
        /// </summary>
        public DateTime? DeletedAt { get; set; }
    }
}


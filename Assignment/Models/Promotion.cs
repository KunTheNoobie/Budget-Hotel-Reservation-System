using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    /// <summary>
    /// Enumeration representing the type of discount applied by a promotion.
    /// </summary>
    public enum DiscountType 
    { 
        Percentage,    // Discount is a percentage (e.g., 10% off)
        FixedAmount    // Discount is a fixed amount (e.g., RM50 off)
    }

    /// <summary>
    /// Represents a promotion code that can be applied to bookings for discounts.
    /// Supports various validation rules including date ranges, minimum amounts, usage limits,
    /// and abuse prevention mechanisms (per phone number, payment card, device, or user account).
    /// </summary>
    public class Promotion
    {
        /// <summary>
        /// Primary key identifier for the promotion.
        /// </summary>
        [Key]
        public int PromotionId { get; set; }

        /// <summary>
        /// Unique promotion code that customers enter to apply the discount (e.g., "WELCOME10", "SUMMER20").
        /// Required field with maximum length of 20 characters.
        /// </summary>
        [Required, StringLength(20)]
        public string Code { get; set; }

        /// <summary>
        /// Description of the promotion, explaining what discount it provides.
        /// Optional field with maximum length of 255 characters.
        /// </summary>
        [StringLength(255)]
        public string? Description { get; set; }

        /// <summary>
        /// Type of discount: Percentage (e.g., 10% off) or FixedAmount (e.g., RM50 off).
        /// </summary>
        [Required]
        public DiscountType Type { get; set; }

        /// <summary>
        /// Value of the discount.
        /// For Percentage type: represents the percentage (e.g., 10 for 10%).
        /// For FixedAmount type: represents the amount in RM (e.g., 50 for RM50).
        /// Stored as decimal(18, 2) for precise calculations.
        /// </summary>
        [Required, Column(TypeName = "decimal(18, 2)")]
        public decimal Value { get; set; }

        /// <summary>
        /// Date and time when the promotion becomes active and can be used.
        /// </summary>
        [Required]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Date and time when the promotion expires and can no longer be used.
        /// </summary>
        [Required]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Flag indicating whether the promotion is currently active.
        /// Inactive promotions cannot be used even if within the date range.
        /// Defaults to true when a new promotion is created.
        /// </summary>
        public bool IsActive { get; set; } = true;

        // ========== Abuse Prevention Settings ==========

        /// <summary>
        /// If true, limits the number of times this promotion can be used per unique phone number.
        /// Helps prevent abuse by limiting usage per customer phone number.
        /// Defaults to true.
        /// </summary>
        [Display(Name = "Limit Per Phone Number")]
        public bool LimitPerPhoneNumber { get; set; } = true;

        /// <summary>
        /// If true, limits the number of times this promotion can be used per unique payment card.
        /// Helps prevent abuse by limiting usage per payment method.
        /// Defaults to true.
        /// </summary>
        [Display(Name = "Limit Per Payment Card")]
        public bool LimitPerPaymentCard { get; set; } = true;

        /// <summary>
        /// If true, limits the number of times this promotion can be used per device or IP address.
        /// Helps prevent abuse by limiting usage per device/location.
        /// Defaults to false.
        /// </summary>
        [Display(Name = "Limit Per Device/IP")]
        public bool LimitPerDevice { get; set; } = false;

        /// <summary>
        /// If true, limits the number of times this promotion can be used per user account.
        /// Helps prevent abuse by limiting usage per registered user.
        /// Defaults to false.
        /// </summary>
        [Display(Name = "Limit Per User Account")]
        public bool LimitPerUserAccount { get; set; } = false;

        /// <summary>
        /// Maximum number of times this promotion can be used per limit type (phone, card, device, or account).
        /// For example, if set to 1 and LimitPerPhoneNumber is true, each phone number can only use it once.
        /// Range: 1 to 1000.
        /// Defaults to 1.
        /// </summary>
        [Display(Name = "Maximum Uses Per Limit")]
        [Range(1, 1000)]
        public int MaxUsesPerLimit { get; set; } = 1;

        // ========== Minimum Requirements ==========

        /// <summary>
        /// Optional minimum number of nights required for the booking to use this promotion.
        /// Null means no minimum night requirement.
        /// Range: 0 to 30 nights.
        /// </summary>
        [Display(Name = "Minimum Stay Nights")]
        [Range(0, 30)]
        public int? MinimumNights { get; set; }

        /// <summary>
        /// Optional minimum booking amount (in RM) required to use this promotion.
        /// Null means no minimum amount requirement.
        /// Stored as decimal(18, 2) for precise currency calculations.
        /// </summary>
        [Display(Name = "Minimum Amount (RM)")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? MinimumAmount { get; set; }

        /// <summary>
        /// Optional maximum total number of times this promotion can be used across all users.
        /// When this limit is reached, the promotion is automatically deactivated.
        /// Null means no total usage limit.
        /// </summary>
        [Display(Name = "Maximum Total Uses")]
        [Range(0, int.MaxValue)]
        public int? MaxTotalUses { get; set; }

        // ========== Soft Delete ==========

        /// <summary>
        /// Soft delete flag - indicates if the promotion has been deleted.
        /// When true, the promotion is hidden from queries but not physically removed from the database.
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Timestamp of when the promotion was soft deleted.
        /// Null if the promotion has not been deleted.
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        // ========== Navigation Properties ==========

        /// <summary>
        /// Navigation property - collection of bookings that used this promotion.
        /// Promotion usage tracking is now stored directly in the Booking table.
        /// </summary>
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
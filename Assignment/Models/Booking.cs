using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    /// <summary>
    /// Enumeration representing the possible statuses of a booking.
    /// </summary>
    public enum BookingStatus 
    { 
        Pending,      // Booking created but not yet confirmed
        Confirmed,    // Booking confirmed and payment processed
        Cancelled,    // Booking was cancelled
        CheckedIn,    // Guest has checked in
        CheckedOut,   // Guest has checked out
        NoShow        // Guest did not show up for the booking
    }

    /// <summary>
    /// Enumeration representing the payment methods available for bookings.
    /// </summary>
    public enum PaymentMethod 
    { 
        CreditCard,    // Payment via credit card
        PayPal,        // Payment via PayPal
        BankTransfer   // Payment via bank transfer
    }

    /// <summary>
    /// Enumeration representing the payment status of a booking.
    /// </summary>
    public enum PaymentStatus 
    { 
        Pending,    // Payment is pending
        Completed,  // Payment has been completed
        Failed,     // Payment failed
        Refunded    // Payment has been refunded
    }

    /// <summary>
    /// Represents a hotel room booking made by a user.
    /// Contains booking details, payment information, and cancellation data.
    /// Payment information is merged into this entity (previously separate Payment table).
    /// </summary>
    public class Booking
    {
        /// <summary>
        /// Primary key identifier for the booking.
        /// </summary>
        [Key]
        public int BookingId { get; set; }

        /// <summary>
        /// Foreign key to the User who made this booking.
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Foreign key to the Room being booked.
        /// </summary>
        [Required]
        public int RoomId { get; set; }

        /// <summary>
        /// Date when the guest is scheduled to check in.
        /// </summary>
        [Required]
        public DateTime CheckInDate { get; set; }

        /// <summary>
        /// Date when the guest is scheduled to check out.
        /// </summary>
        [Required]
        public DateTime CheckOutDate { get; set; }

        /// <summary>
        /// Date and time when the booking was created.
        /// Defaults to the current date/time.
        /// </summary>
        [Required]
        public DateTime BookingDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Total price of the booking in RM (Malaysian Ringgit).
        /// Stored as decimal(18, 2) for precise currency calculations.
        /// </summary>
        [Required, Column(TypeName = "decimal(18, 2)")]
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// Current status of the booking (Pending, Confirmed, Cancelled, etc.).
        /// Defaults to Pending when a booking is first created.
        /// </summary>
        [Required]
        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        /// <summary>
        /// Optional foreign key to a Promotion that was applied to this booking.
        /// Null if no promotion was used.
        /// </summary>
        public int? PromotionId { get; set; }

        /// <summary>
        /// Navigation property - the user who made this booking.
        /// </summary>
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        /// <summary>
        /// Navigation property - the room being booked.
        /// </summary>
        [ForeignKey("RoomId")]
        public virtual Room? Room { get; set; }

        /// <summary>
        /// Navigation property - the promotion applied to this booking (if any).
        /// </summary>
        [ForeignKey("PromotionId")]
        public virtual Promotion? Promotion { get; set; }

        // ========== Payment Information (Merged from Payment table) ==========
        
        /// <summary>
        /// Amount paid for this booking. May differ from TotalPrice if partial payment was made.
        /// Stored as decimal(18, 2) for precise currency calculations.
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? PaymentAmount { get; set; }

        /// <summary>
        /// Method used to make the payment (CreditCard, PayPal, BankTransfer).
        /// Null if payment has not been made yet.
        /// </summary>
        public PaymentMethod? PaymentMethod { get; set; }

        /// <summary>
        /// Status of the payment (Pending, Completed, Failed, Refunded).
        /// Defaults to Pending when booking is created.
        /// </summary>
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        /// <summary>
        /// Unique transaction identifier from the payment processor.
        /// Auto-generated when payment is processed.
        /// </summary>
        [StringLength(255)]
        public string? TransactionId { get; set; }

        /// <summary>
        /// Date and time when the payment was processed.
        /// Null if payment has not been made yet.
        /// </summary>
        public DateTime? PaymentDate { get; set; }

        // ========== Review Relationship ==========

        /// <summary>
        /// Navigation property - collection of reviews for this booking.
        /// A booking can have multiple reviews (though typically one per booking).
        /// </summary>
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

        // ========== Cancellation Information (Merged from BookingCancellation table) ==========

        /// <summary>
        /// Date and time when the booking was cancelled.
        /// Null if the booking has not been cancelled.
        /// </summary>
        public DateTime? CancellationDate { get; set; }

        /// <summary>
        /// Reason provided for cancelling the booking.
        /// Maximum length of 500 characters.
        /// </summary>
        [StringLength(500)]
        public string? CancellationReason { get; set; }

        /// <summary>
        /// Amount refunded to the customer after cancellation.
        /// Stored as decimal(18, 2) for precise currency calculations.
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? RefundAmount { get; set; }

        // ========== Soft Delete ==========

        /// <summary>
        /// Soft delete flag - indicates if the booking has been deleted.
        /// When true, the booking is hidden from queries but not physically removed from the database.
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Timestamp of when the booking was soft deleted.
        /// Null if the booking has not been deleted.
        /// </summary>
        public DateTime? DeletedAt { get; set; }
    }
}
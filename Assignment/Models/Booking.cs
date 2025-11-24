using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    public enum BookingStatus { Pending, Confirmed, Cancelled, CheckedIn, CheckedOut, NoShow }
    public enum PaymentMethod { CreditCard, PayPal, BankTransfer }
    public enum PaymentStatus { Pending, Completed, Failed, Refunded }

    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int RoomId { get; set; }

        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        [Required]
        public DateTime BookingDate { get; set; } = DateTime.Now;

        [Required, Column(TypeName = "decimal(18, 2)")]
        public decimal TotalPrice { get; set; }

        [Required]
        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        public int? PromotionId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
        [ForeignKey("RoomId")]
        public virtual Room? Room { get; set; }
        [ForeignKey("PromotionId")]
        public virtual Promotion? Promotion { get; set; }

        // Merged from Payment (one-to-one relationship removed)
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? PaymentAmount { get; set; }

        public PaymentMethod? PaymentMethod { get; set; }

        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        [StringLength(255)]
        public string? TransactionId { get; set; }

        public DateTime? PaymentDate { get; set; }

        // Review relationship (one-to-many: one booking can have one review, but reviews are separate entities)
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

        // Merged from BookingCancellation
        public DateTime? CancellationDate { get; set; }

        [StringLength(500)]
        public string? CancellationReason { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? RefundAmount { get; set; }

        // Soft Delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }
}
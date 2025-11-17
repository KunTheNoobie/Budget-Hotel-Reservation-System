using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    public enum PaymentMethod { CreditCard, PayPal, BankTransfer }
    public enum PaymentStatus { Pending, Completed, Failed, Refunded }

    public class Payment
    {
        [Key, ForeignKey("Booking")]
        public int BookingId { get; set; }

        [Required, Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        [Required]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        [StringLength(255)]
        public string? TransactionId { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        public virtual Booking Booking { get; set; }
    }
}
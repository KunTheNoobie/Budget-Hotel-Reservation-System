using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    public class BookingCancellation
    {
        [Key, ForeignKey("Booking")]
        public int BookingId { get; set; }

        [Required]
        public DateTime CancellationDate { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string? Reason { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal RefundAmount { get; set; }

        public virtual Booking Booking { get; set; }
    }
}
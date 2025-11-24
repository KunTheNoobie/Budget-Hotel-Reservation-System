using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    public class PromotionUsage
    {
        [Key]
        public int PromotionUsageId { get; set; }

        [Required]
        public int PromotionId { get; set; }

        [ForeignKey("PromotionId")]
        public virtual Promotion? Promotion { get; set; }

        [Required]
        public int BookingId { get; set; }

        [ForeignKey("BookingId")]
        public virtual Booking? Booking { get; set; }

        // Track by phone number (encrypted)
        [StringLength(255)]
        public string? PhoneNumberHash { get; set; }

        // Track by payment card (last 4 digits + hash)
        [StringLength(100)]
        public string? CardIdentifier { get; set; }

        // Track by device/IP
        [StringLength(100)]
        public string? DeviceFingerprint { get; set; }

        [StringLength(50)]
        public string? IpAddress { get; set; }

        public DateTime UsedAt { get; set; } = DateTime.Now;

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        // Soft Delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }
}


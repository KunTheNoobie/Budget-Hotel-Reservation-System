using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    public class Review
    {
        [Key, ForeignKey("Booking")]
        public int BookingId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required, Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(2000)]
        public string? Comment { get; set; }

        public DateTime ReviewDate { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        public virtual Booking Booking { get; set; }
    }
}
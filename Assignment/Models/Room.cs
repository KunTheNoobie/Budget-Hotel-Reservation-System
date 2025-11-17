using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    public enum RoomStatus { Available, Occupied, UnderMaintenance, Cleaning }

    public class Room
    {
        [Key]
        public int RoomId { get; set; }

        [Required, StringLength(10)]
        public string RoomNumber { get; set; }

        [Required]
        public int RoomTypeId { get; set; }

        [Required]
        public RoomStatus Status { get; set; } = RoomStatus.Available;

        [ForeignKey("RoomTypeId")]
        public virtual RoomType RoomType { get; set; }

        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
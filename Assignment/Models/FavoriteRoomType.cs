using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    public class FavoriteRoomType
    {
        [Key]
        public int FavoriteId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int RoomTypeId { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("RoomTypeId")]
        public virtual RoomType? RoomType { get; set; }

        // Soft Delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }
}


using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    public class RoomType
    {
        [Key]
        public int RoomTypeId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required, Range(1, 10)]
        public int Occupancy { get; set; }

        [Required, Column(TypeName = "decimal(18, 2)"), Range(0, 99999.99)]
        public decimal BasePrice { get; set; }

        [Required]
        public int HotelId { get; set; }

        [ForeignKey("HotelId")]
        public virtual Hotel? Hotel { get; set; }

        // Soft Delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
        public virtual ICollection<RoomImage> RoomImages { get; set; } = new List<RoomImage>();
        public virtual ICollection<RoomTypeAmenity> RoomTypeAmenities { get; set; } = new List<RoomTypeAmenity>();
        public virtual ICollection<FavoriteRoomType> FavoriteRoomTypes { get; set; } = new List<FavoriteRoomType>();
    }
}
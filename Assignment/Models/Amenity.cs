using System.ComponentModel.DataAnnotations;
namespace Assignment.Models
{
    public class Amenity
    {
        [Key]
        public int AmenityId { get; set; }
        
        [Required, StringLength(100)]
        public string Name { get; set; }

        [StringLength(255)]
        public string? ImageUrl { get; set; }

        // Soft Delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        public virtual ICollection<RoomTypeAmenity> RoomTypeAmenities { get; set; } = new List<RoomTypeAmenity>();
    }
}

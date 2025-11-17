using System.ComponentModel.DataAnnotations;
namespace Assignment.Models
{
    public class Amenity
    {
        [Key]
        public int AmenityId { get; set; }
        
        [Required, StringLength(100)]
        public string Name { get; set; }

        public virtual ICollection<RoomTypeAmenity> RoomTypeAmenities { get; set; } = new List<RoomTypeAmenity>();
    }
}

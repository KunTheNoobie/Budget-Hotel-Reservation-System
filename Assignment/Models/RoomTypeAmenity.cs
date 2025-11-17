using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    public class RoomTypeAmenity
    {
        public int RoomTypeAmenityId { get; set; }
        public int RoomTypeId { get; set; }
        public int AmenityId { get; set; }

        [ForeignKey("RoomTypeId")]
        public virtual RoomType RoomType { get; set; }

        [ForeignKey("AmenityId")]
        public virtual Amenity Amenity { get; set; }
    }
}
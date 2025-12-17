using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    /// <summary>
    /// Junction entity representing the many-to-many relationship between RoomTypes and Amenities.
    /// Links room types to their associated amenities (e.g., a "Deluxe Double Room" has "Free Wi-Fi", "Air Conditioning", etc.).
    /// This allows multiple room types to share the same amenities and room types to have multiple amenities.
    /// </summary>
    public class RoomTypeAmenity
    {
        /// <summary>
        /// Primary key identifier for the room type-amenity relationship.
        /// </summary>
        public int RoomTypeAmenityId { get; set; }

        /// <summary>
        /// Foreign key to the RoomType that has this amenity.
        /// </summary>
        public int RoomTypeId { get; set; }

        /// <summary>
        /// Foreign key to the Amenity that is associated with this room type.
        /// </summary>
        public int AmenityId { get; set; }

        /// <summary>
        /// Navigation property - the room type that has this amenity.
        /// </summary>
        [ForeignKey("RoomTypeId")]
        public virtual RoomType RoomType { get; set; }

        /// <summary>
        /// Navigation property - the amenity associated with this room type.
        /// </summary>
        [ForeignKey("AmenityId")]
        public virtual Amenity Amenity { get; set; }
    }
}
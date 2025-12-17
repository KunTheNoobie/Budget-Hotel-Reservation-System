using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    /// <summary>
    /// Represents a type/category of room in a hotel (e.g., "Standard Single", "Deluxe Double", "Executive Suite").
    /// Defines the characteristics, pricing, and amenities for a category of rooms.
    /// Multiple physical rooms (Room entities) can belong to the same room type.
    /// </summary>
    public class RoomType
    {
        /// <summary>
        /// Primary key identifier for the room type.
        /// </summary>
        [Key]
        public int RoomTypeId { get; set; }

        /// <summary>
        /// Name of the room type (e.g., "Standard Single Room", "Deluxe Double Room").
        /// Required field with maximum length of 100 characters.
        /// </summary>
        [Required, StringLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// Detailed description of the room type, including features and amenities.
        /// Optional field with maximum length of 1000 characters.
        /// </summary>
        [StringLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Maximum number of guests that can occupy this room type.
        /// Range: 1 to 10 guests.
        /// </summary>
        [Required, Range(1, 10)]
        public int Occupancy { get; set; }

        /// <summary>
        /// Base price per night for this room type in RM (Malaysian Ringgit).
        /// Stored as decimal(18, 2) for precise currency calculations.
        /// Range: 0 to 99,999.99 RM.
        /// </summary>
        [Required, Column(TypeName = "decimal(18, 2)"), Range(0, 99999.99)]
        public decimal BasePrice { get; set; }

        /// <summary>
        /// Foreign key to the Hotel that offers this room type.
        /// </summary>
        [Required]
        public int HotelId { get; set; }

        /// <summary>
        /// Navigation property - the hotel that offers this room type.
        /// </summary>
        [ForeignKey("HotelId")]
        public virtual Hotel? Hotel { get; set; }

        /// <summary>
        /// Soft delete flag - indicates if the room type has been deleted.
        /// When true, the room type is hidden from queries but not physically removed from the database.
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Timestamp of when the room type was soft deleted.
        /// Null if the room type has not been deleted.
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Navigation property - collection of physical rooms of this type.
        /// Multiple Room entities can belong to the same RoomType.
        /// </summary>
        public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();

        /// <summary>
        /// Navigation property - collection of images for this room type.
        /// Room types can have multiple images to showcase different views and features.
        /// </summary>
        public virtual ICollection<RoomImage> RoomImages { get; set; } = new List<RoomImage>();

        /// <summary>
        /// Navigation property - collection of amenities associated with this room type.
        /// Represents a many-to-many relationship through RoomTypeAmenity.
        /// </summary>
        public virtual ICollection<RoomTypeAmenity> RoomTypeAmenities { get; set; } = new List<RoomTypeAmenity>();
    }
}
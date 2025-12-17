using System.ComponentModel.DataAnnotations;

namespace Assignment.Models
{
    /// <summary>
    /// Represents an amenity or facility that can be associated with room types.
    /// Examples include: Free Wi-Fi, Air Conditioning, Flat-Screen TV, etc.
    /// </summary>
    public class Amenity
    {
        /// <summary>
        /// Primary key identifier for the amenity.
        /// </summary>
        [Key]
        public int AmenityId { get; set; }
        
        /// <summary>
        /// Name of the amenity (e.g., "Free Wi-Fi", "Air Conditioning").
        /// Required field with maximum length of 100 characters.
        /// </summary>
        [Required, StringLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// Optional URL to an image representing the amenity.
        /// Used for displaying amenity icons in the UI.
        /// </summary>
        [StringLength(255)]
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Soft delete flag - indicates if the amenity has been deleted.
        /// When true, the amenity is hidden from queries but not physically removed from the database.
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Timestamp of when the amenity was soft deleted.
        /// Null if the amenity has not been deleted.
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Navigation property - collection of room types that have this amenity.
        /// Represents a many-to-many relationship through RoomTypeAmenity.
        /// </summary>
        public virtual ICollection<RoomTypeAmenity> RoomTypeAmenities { get; set; } = new List<RoomTypeAmenity>();
    }
}

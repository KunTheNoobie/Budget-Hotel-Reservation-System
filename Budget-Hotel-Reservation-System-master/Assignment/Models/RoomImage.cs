using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    /// <summary>
    /// Represents an image associated with a room type.
    /// Room types can have multiple images (e.g., room photos, bathroom, view, etc.)
    /// to showcase the room to potential customers.
    /// </summary>
    public class RoomImage
    {
        /// <summary>
        /// Primary key identifier for the room image.
        /// </summary>
        [Key]
        public int ImageId { get; set; }

        /// <summary>
        /// Foreign key to the RoomType that this image belongs to.
        /// </summary>
        [Required]
        public int RoomTypeId { get; set; }

        /// <summary>
        /// URL to the image file.
        /// Can be a local path or external URL (e.g., from a CDN or image hosting service).
        /// Required field with maximum length of 255 characters.
        /// </summary>
        [Required, StringLength(255)]
        public string ImageUrl { get; set; }

        /// <summary>
        /// Optional caption/description for the image.
        /// Used for accessibility (alt text) and display purposes.
        /// Maximum length of 150 characters.
        /// </summary>
        [StringLength(150)]
        public string? Caption { get; set; }

        /// <summary>
        /// Soft delete flag - indicates if the room image has been deleted.
        /// When true, the image is hidden from queries but not physically removed from the database.
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Timestamp of when the room image was soft deleted.
        /// Null if the image has not been deleted.
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Navigation property - the room type that this image belongs to.
        /// </summary>
        [ForeignKey("RoomTypeId")]
        public virtual RoomType? RoomType { get; set; }
    }
}
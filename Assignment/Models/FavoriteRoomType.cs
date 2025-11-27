using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    /// <summary>
    /// Represents a favorite/wishlist entry where a user saves a room type for later viewing.
    /// Implements a many-to-many relationship between Users and RoomTypes for the favorites feature.
    /// </summary>
    public class FavoriteRoomType
    {
        /// <summary>
        /// Primary key identifier for the favorite entry.
        /// </summary>
        [Key]
        public int FavoriteId { get; set; }

        /// <summary>
        /// Foreign key to the User who added this room type to their favorites.
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Foreign key to the RoomType that was added to favorites.
        /// </summary>
        [Required]
        public int RoomTypeId { get; set; }

        /// <summary>
        /// Date and time when the room type was added to the user's favorites.
        /// Defaults to the current date/time.
        /// </summary>
        public DateTime AddedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Navigation property - the user who added this room type to favorites.
        /// </summary>
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        /// <summary>
        /// Navigation property - the room type that was favorited.
        /// </summary>
        [ForeignKey("RoomTypeId")]
        public virtual RoomType? RoomType { get; set; }

        /// <summary>
        /// Soft delete flag - indicates if the favorite entry has been deleted.
        /// When true, the favorite is hidden from queries but not physically removed from the database.
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Timestamp of when the favorite entry was soft deleted.
        /// Null if the favorite has not been deleted.
        /// </summary>
        public DateTime? DeletedAt { get; set; }
    }
}


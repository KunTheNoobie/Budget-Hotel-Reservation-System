using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    /// <summary>
    /// Enumeration representing the current status of a room.
    /// </summary>
    public enum RoomStatus 
    { 
        Available,          // Room is available for booking
        Occupied,           // Room is currently occupied by a guest
        UnderMaintenance,   // Room is being repaired or maintained
        Cleaning            // Room is being cleaned
    }

    /// <summary>
    /// Represents a physical room in a hotel.
    /// Each room has a unique room number and belongs to a specific room type.
    /// Tracks the current status of the room (available, occupied, maintenance, etc.)
    /// and maintains a history of bookings for that room.
    /// </summary>
    public class Room
    {
        /// <summary>
        /// Primary key identifier for the room.
        /// </summary>
        [Key]
        public int RoomId { get; set; }

        /// <summary>
        /// Unique room number/identifier (e.g., "101", "202", "301").
        /// Required field with maximum length of 10 characters.
        /// Used for identification and display purposes.
        /// </summary>
        [Required, StringLength(10)]
        public string RoomNumber { get; set; }

        /// <summary>
        /// Foreign key to the RoomType that defines the characteristics of this room.
        /// All rooms of the same type share the same amenities, price, and features.
        /// </summary>
        [Required]
        public int RoomTypeId { get; set; }

        /// <summary>
        /// Current status of the room (Available, Occupied, UnderMaintenance, Cleaning).
        /// Used to determine if the room can be booked.
        /// Defaults to Available when a new room is created.
        /// </summary>
        [Required]
        public RoomStatus Status { get; set; } = RoomStatus.Available;

        /// <summary>
        /// Navigation property - the room type that defines this room's characteristics.
        /// </summary>
        [ForeignKey("RoomTypeId")]
        public virtual RoomType? RoomType { get; set; }

        /// <summary>
        /// Soft delete flag - indicates if the room has been deleted.
        /// When true, the room is hidden from queries but not physically removed from the database.
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Timestamp of when the room was soft deleted.
        /// Null if the room has not been deleted.
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Navigation property - collection of bookings made for this room.
        /// Maintains a history of all bookings for this specific room.
        /// </summary>
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
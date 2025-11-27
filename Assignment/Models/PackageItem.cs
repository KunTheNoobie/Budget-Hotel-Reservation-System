using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    /// <summary>
    /// Represents an individual item (room type or service) included in a package.
    /// This is a junction entity that links Packages to RoomTypes and Services,
    /// allowing packages to contain multiple room types and services with specified quantities.
    /// </summary>
    public class PackageItem
    {
        /// <summary>
        /// Primary key identifier for the package item.
        /// </summary>
        public int PackageItemId { get; set; }

        /// <summary>
        /// Foreign key to the Package that contains this item.
        /// </summary>
        public int PackageId { get; set; }

        /// <summary>
        /// Optional foreign key to a RoomType included in this package.
        /// Null if this item is a service instead of a room type.
        /// </summary>
        public int? RoomTypeId { get; set; }

        /// <summary>
        /// Optional foreign key to a Service included in this package.
        /// Null if this item is a room type instead of a service.
        /// </summary>
        public int? ServiceId { get; set; }

        /// <summary>
        /// Quantity of this item included in the package.
        /// For room types, this typically represents the number of nights.
        /// For services, this represents how many times the service is included.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Soft delete flag - indicates if the package item has been deleted.
        /// When true, the item is hidden from queries but not physically removed from the database.
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Timestamp of when the package item was soft deleted.
        /// Null if the item has not been deleted.
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Navigation property - the package that contains this item.
        /// </summary>
        [ForeignKey("PackageId")]
        public virtual Package Package { get; set; }

        /// <summary>
        /// Navigation property - the room type included in this package item (if applicable).
        /// </summary>
        [ForeignKey("RoomTypeId")]
        public virtual RoomType? RoomType { get; set; }

        /// <summary>
        /// Navigation property - the service included in this package item (if applicable).
        /// </summary>
        [ForeignKey("ServiceId")]
        public virtual Service? Service { get; set; }
    }
}
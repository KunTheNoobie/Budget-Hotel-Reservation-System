using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    /// <summary>
    /// Represents a package deal that bundles room types and services together at a discounted price.
    /// Examples: "Kuala Lumpur City Explorer", "Honeymoon Bliss", "Business Traveler", etc.
    /// Packages can include multiple room nights and various services (breakfast, tours, transfers, etc.).
    /// </summary>
    public class Package
    {
        /// <summary>
        /// Primary key identifier for the package.
        /// </summary>
        [Key]
        public int PackageId { get; set; }

        /// <summary>
        /// Name of the package (e.g., "Kuala Lumpur City Explorer").
        /// Required field with maximum length of 150 characters.
        /// </summary>
        [Required, StringLength(150)]
        public string Name { get; set; }

        /// <summary>
        /// Detailed description of what the package includes.
        /// Optional field with maximum length of 1000 characters.
        /// </summary>
        [StringLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Total price of the package in RM (Malaysian Ringgit).
        /// Stored as decimal(18, 2) for precise currency calculations.
        /// This is the bundled price, which is typically lower than purchasing items separately.
        /// </summary>
        [Required, Column(TypeName = "decimal(18, 2)")]
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// URL to an image representing the package.
        /// Optional field with maximum length of 255 characters.
        /// Used for displaying the package in the UI.
        /// </summary>
        [StringLength(255)]
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Flag indicating whether the package is currently active and available for booking.
        /// Inactive packages are hidden from customers but not deleted.
        /// Defaults to true when a new package is created.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Soft delete flag - indicates if the package has been deleted.
        /// When true, the package is hidden from queries but not physically removed from the database.
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Timestamp of when the package was soft deleted.
        /// Null if the package has not been deleted.
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Navigation property - collection of items (room types and services) included in this package.
        /// Each PackageItem represents one component of the package with its quantity.
        /// </summary>
        public virtual ICollection<PackageItem> PackageItems { get; set; } = new List<PackageItem>();
    }
}
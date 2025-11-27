using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    /// <summary>
    /// Represents an additional service that can be purchased or included in packages.
    /// Examples include: Airport Transfer, Breakfast Buffet, Spa Treatment, Car Rental, etc.
    /// Services can be added to bookings or bundled into packages.
    /// </summary>
    public class Service
    {
        /// <summary>
        /// Primary key identifier for the service.
        /// </summary>
        [Key]
        public int ServiceId { get; set; }

        /// <summary>
        /// Name of the service (e.g., "Airport Transfer", "Breakfast Buffet").
        /// Required field with maximum length of 100 characters.
        /// </summary>
        [Required, StringLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// Detailed description of what the service includes.
        /// Optional field with maximum length of 500 characters.
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Price of the service in RM (Malaysian Ringgit).
        /// Stored as decimal(18, 2) for precise currency calculations.
        /// </summary>
        [Required, Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        /// <summary>
        /// Soft delete flag - indicates if the service has been deleted.
        /// When true, the service is hidden from queries but not physically removed from the database.
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Timestamp of when the service was soft deleted.
        /// Null if the service has not been deleted.
        /// </summary>
        public DateTime? DeletedAt { get; set; }
    }
}
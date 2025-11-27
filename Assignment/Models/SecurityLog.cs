using System.ComponentModel.DataAnnotations;

namespace Assignment.Models
{
    /// <summary>
    /// Represents a security event log entry for audit and monitoring purposes.
    /// Records security-related actions such as login, logout, failed login attempts,
    /// password changes, and other security events for compliance and security analysis.
    /// </summary>
    public class SecurityLog
    {
        /// <summary>
        /// Primary key identifier for the security log entry.
        /// </summary>
        [Key]
        public int LogId { get; set; }

        /// <summary>
        /// Optional foreign key to the User associated with this security event.
        /// Null if the event is not associated with a specific user (e.g., system events).
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Type of security action that occurred (e.g., "Login", "Logout", "FailedLogin", "PasswordReset", etc.).
        /// Required field with maximum length of 50 characters.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Action { get; set; } // Login, Logout, FailedLogin, etc.

        /// <summary>
        /// IP address from which the security event originated.
        /// Used for security monitoring and fraud detection.
        /// Optional field with maximum length of 50 characters (supports IPv6).
        /// </summary>
        [StringLength(50)]
        public string? IPAddress { get; set; }

        /// <summary>
        /// Date and time when the security event occurred.
        /// Defaults to the current date/time.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>
        /// Additional details or context about the security event.
        /// Can include error messages, user agent strings, or other relevant information.
        /// Optional field with maximum length of 500 characters.
        /// </summary>
        [StringLength(500)]
        public string? Details { get; set; }

        /// <summary>
        /// Soft delete flag - indicates if the security log entry has been deleted.
        /// When true, the log entry is hidden from queries but not physically removed from the database.
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Timestamp of when the security log entry was soft deleted.
        /// Null if the log entry has not been deleted.
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Navigation property - the user associated with this security event (if applicable).
        /// </summary>
        public virtual User? User { get; set; }
    }
}

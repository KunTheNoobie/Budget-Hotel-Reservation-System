using System.ComponentModel.DataAnnotations;

namespace Assignment.Models
{
    /// <summary>
    /// Represents a login attempt record for security and audit purposes.
    /// Tracks all login attempts (both successful and failed) to monitor security events
    /// and implement account lockout functionality after multiple failed attempts.
    /// </summary>
    public class LoginAttempt
    {
        /// <summary>
        /// Primary key identifier for the login attempt record.
        /// </summary>
        [Key]
        public int AttemptId { get; set; }

        /// <summary>
        /// Email address used in the login attempt.
        /// Required field with maximum length of 100 characters.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Email { get; set; }

        /// <summary>
        /// Date and time when the login attempt occurred.
        /// Defaults to the current date/time.
        /// </summary>
        [Required]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>
        /// Flag indicating whether the login attempt was successful.
        /// True if login succeeded, false if it failed (wrong password, account locked, etc.).
        /// </summary>
        public bool WasSuccessful { get; set; }

        /// <summary>
        /// IP address from which the login attempt was made.
        /// Optional field with maximum length of 45 characters (supports IPv6).
        /// Used for security monitoring and fraud detection.
        /// </summary>
        [StringLength(45)]
        public string? IpAddress { get; set; }
    }
}
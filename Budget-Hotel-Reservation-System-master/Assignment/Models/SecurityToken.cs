using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    /// <summary>
    /// Enumeration representing the type of security token.
    /// </summary>
    public enum TokenType 
    { 
        PasswordReset,        // Token for resetting a forgotten password
        EmailVerification     // Token for verifying email address during registration
    }

    /// <summary>
    /// Represents a security token used for password reset and email verification.
    /// Tokens are time-limited and single-use to ensure security.
    /// When a user requests a password reset or needs to verify their email,
    /// a unique token is generated and sent to them via email.
    /// </summary>
    public class SecurityToken
    {
        /// <summary>
        /// Primary key identifier for the security token.
        /// </summary>
        [Key]
        public int TokenId { get; set; }

        /// <summary>
        /// The unique token value (typically a GUID or cryptographically random string).
        /// This is the value sent to the user via email and used to verify their identity.
        /// Required field with maximum length of 256 characters.
        /// </summary>
        [Required]
        [StringLength(256)]
        public string TokenValue { get; set; }

        /// <summary>
        /// Foreign key to the User this token belongs to.
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Type of token: PasswordReset or EmailVerification.
        /// Determines what action the token can be used for.
        /// </summary>
        [Required]
        public TokenType Type { get; set; }

        /// <summary>
        /// Date and time when the token expires and becomes invalid.
        /// Tokens typically expire after a set period (e.g., 24 hours) for security.
        /// </summary>
        [Required]
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// Flag indicating whether the token has already been used.
        /// Tokens are single-use: once used, they cannot be used again.
        /// Defaults to false when a new token is created.
        /// </summary>
        public bool IsUsed { get; set; } = false;

        /// <summary>
        /// Navigation property - the user this token belongs to.
        /// </summary>
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
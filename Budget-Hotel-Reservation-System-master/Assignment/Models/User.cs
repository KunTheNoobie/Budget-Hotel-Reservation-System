using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Assignment.Services;

namespace Assignment.Models
{
    /// <summary>
    /// Enumeration representing the role/access level of a user in the system.
    /// </summary>
    public enum UserRole 
    { 
        Admin,      // Full system access - can manage all aspects of the system
        Manager,    // Full system access (same as Admin)
        Staff,      // Limited admin access - can manage bookings and some content
        Customer    // Standard user - can make bookings and manage their profile
    }

    /// <summary>
    /// Represents a user account in the system (admin, staff, or customer).
    /// Stores user authentication information, profile data, preferences, and relationships
    /// to bookings, reviews, and favorites.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Primary key identifier for the user.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Email address of the user, used for login and communication.
        /// Required field, must be a valid email format, maximum length of 100 characters.
        /// Must be unique across all users.
        /// </summary>
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        /// <summary>
        /// Full name of the user.
        /// Required field with maximum length of 100 characters.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        /// <summary>
        /// Hashed password using BCrypt algorithm.
        /// Never store plain text passwords - always use hashing.
        /// Required field.
        /// </summary>
        [Required]
        public string PasswordHash { get; set; }

        /// <summary>
        /// Encrypted phone number of the user.
        /// Stored as encrypted string (not plain text) for privacy and security.
        /// Maximum length of 255 characters to accommodate encrypted data.
        /// </summary>
        [StringLength(255)] // Increased length for encrypted string
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Decrypted phone number property (not stored in database).
        /// Automatically encrypts/decrypts when getting or setting the value.
        /// Used for displaying and editing phone numbers in the UI.
        /// </summary>
        [NotMapped]
        public string? DecryptedPhoneNumber 
        { 
            get => EncryptionService.Decrypt(PhoneNumber ?? "");
            set => PhoneNumber = EncryptionService.Encrypt(value ?? "");
        }

        /// <summary>
        /// Role/access level of the user (Admin, Manager, Staff, or Customer).
        /// Determines what actions the user can perform in the system.
        /// Defaults to Customer when a new user is created.
        /// </summary>
        [Required]
        public UserRole Role { get; set; } = UserRole.Customer;

        /// <summary>
        /// Flag indicating whether the user's email address has been verified.
        /// Users must verify their email before they can use certain features.
        /// Defaults to false when a new user is created.
        /// </summary>
        public bool IsEmailVerified { get; set; } = false;

        /// <summary>
        /// Flag indicating whether the user account is active.
        /// Inactive accounts cannot log in.
        /// Defaults to true when a new user is created.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Date and time when the user account was created.
        /// Defaults to the current date/time.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// URL to the user's profile picture/avatar.
        /// Optional field with maximum length of 255 characters.
        /// </summary>
        [StringLength(255)]
        public string? ProfilePictureUrl { get; set; }

        /// <summary>
        /// User's biography or personal description.
        /// Optional field with maximum length of 500 characters.
        /// </summary>
        [StringLength(500)]
        public string? Bio { get; set; }

        /// <summary>
        /// User's preferred language for the UI (e.g., "en-US", "ms-MY").
        /// Defaults to "en-US" (English - United States).
        /// Maximum length of 10 characters.
        /// </summary>
        [StringLength(10)]
        public string PreferredLanguage { get; set; } = "en-US";

        /// <summary>
        /// User's preferred theme/color scheme for the UI (e.g., "Default", "Dark", "Light").
        /// Defaults to "Default".
        /// Maximum length of 50 characters.
        /// </summary>
        [StringLength(50)]
        public string Theme { get; set; } = "Default";

        /// <summary>
        /// Soft delete flag - indicates if the user account has been deleted.
        /// When true, the user is hidden from queries but not physically removed from the database.
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Timestamp of when the user account was soft deleted.
        /// Null if the user account has not been deleted.
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Hotel ID that the user is assigned to (for Manager and Staff roles).
        /// Null for Admin (who can access all hotels) and Customer roles.
        /// </summary>
        public int? HotelId { get; set; }

        // ========== Navigation Properties ==========

        /// <summary>
        /// Navigation property - the hotel that this user is assigned to (for Manager/Staff).
        /// </summary>
        [ForeignKey("HotelId")]
        public virtual Hotel? Hotel { get; set; }

        /// <summary>
        /// Navigation property - collection of bookings made by this user.
        /// </summary>
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

        // Note: Reviews are no longer directly linked to User.
        // Reviews: Get user info from Review.Booking.UserId
    }
}
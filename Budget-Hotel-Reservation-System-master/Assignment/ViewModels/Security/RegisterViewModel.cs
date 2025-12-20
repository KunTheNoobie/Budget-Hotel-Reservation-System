using System.ComponentModel.DataAnnotations;
using Assignment.Models;

namespace Assignment.ViewModels.Security
{
    /// <summary>
    /// View model for the user registration form.
    /// Contains validation for user registration including password strength requirements
    /// and a math captcha for spam prevention.
    /// Used by SecurityController.Register action for new user registration.
    /// </summary>
    public class RegisterViewModel
    {
        /// <summary>
        /// User's full name for the account.
        /// Required field - cannot be empty.
        /// Maximum length: 100 characters.
        /// </summary>
        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// User's email address (used as username for login).
        /// Must be unique - cannot be registered twice.
        /// Required field - must be valid email format.
        /// Maximum length: 100 characters.
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User's password for account authentication.
        /// Password requirements (enforced by RegularExpression):
        /// - Minimum 8 characters
        /// - Must contain at least one uppercase letter (A-Z)
        /// - Must contain at least one lowercase letter (a-z)
        /// - Must contain at least one digit (0-9)
        /// - Must contain at least one special character (@$!%*?&)
        /// Password is hashed using BCrypt before storage (never stored in plain text).
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "Password must be 8-100 characters", MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", 
            ErrorMessage = "Password must contain uppercase, lowercase, number, and special character (@$!%*?&)")]
        public string Password { get; set; }

        /// <summary>
        /// Password confirmation field - must match Password field.
        /// Used to prevent typos in password entry.
        /// Required field - must match Password exactly.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// User's phone number (optional field).
        /// If provided, must be valid phone number format.
        /// Phone number is encrypted using AES-256 before storage in database.
        /// Maximum length: 20 characters.
        /// Used for promotion abuse prevention (if provided).
        /// </summary>
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [Display(Name = "Phone Number (Optional)")]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Answer to the math captcha question (e.g., "What is 5 + 3?").
        /// Used for spam prevention - prevents automated bot registrations.
        /// Required field - must match the correct answer stored in TempData.
        /// The captcha question is generated randomly on page load (two numbers between 1-9).
        /// </summary>
        [Required(ErrorMessage = "Please answer the security question")]
        [Display(Name = "Security Question")]
        public int CaptchaAnswer { get; set; }
    }
}


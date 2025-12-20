using System.ComponentModel.DataAnnotations;

namespace Assignment.ViewModels.Security
{
    /// <summary>
    /// View model for the password reset form.
    /// Used when a user resets their password using a security token received via email.
    /// Contains token validation and password strength requirements.
    /// Used by SecurityController.ResetPassword action.
    /// </summary>
    public class ResetPasswordViewModel
    {
        /// <summary>
        /// Security token received via email for password reset.
        /// This token is generated when user requests password reset and sent via email.
        /// Token must be valid, not expired, and not already used.
        /// Required field - links the reset request to the user account.
        /// </summary>
        [Required]
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// User's email address for password reset.
        /// Required field - must match the email associated with the token.
        /// Used to identify which user account to reset the password for.
        /// </summary>
        [Required]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// New password for the user account.
        /// Password requirements (enforced by RegularExpression):
        /// - Minimum 8 characters
        /// - Must contain at least one uppercase letter (A-Z)
        /// - Must contain at least one lowercase letter (a-z)
        /// - Must contain at least one digit (0-9)
        /// - Must contain at least one special character (@$!%*?&)
        /// Password is hashed using BCrypt before storage (never stored in plain text).
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, ErrorMessage = "Password must be 8-100 characters", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Password must contain uppercase, lowercase, number, and special character (@$!%*?&)")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Password confirmation field - must match Password field exactly.
        /// Used to prevent typos in password entry.
        /// Required field - must match Password exactly.
        /// </summary>
        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}


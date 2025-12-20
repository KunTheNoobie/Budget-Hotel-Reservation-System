using System.ComponentModel.DataAnnotations;

namespace Assignment.ViewModels.Security
{
    /// <summary>
    /// View model for the forgot password form.
    /// Used when a user requests a password reset via email.
    /// Used by SecurityController.ForgotPassword action.
    /// </summary>
    public class ForgotPasswordViewModel
    {
        /// <summary>
        /// User's email address for password reset request.
        /// Required field - must be valid email format.
        /// System will send a 6-digit OTP code to this email if the email exists in the database.
        /// For security, the system does not reveal whether the email exists or not.
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;
    }
}


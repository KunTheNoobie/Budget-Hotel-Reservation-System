using System.ComponentModel.DataAnnotations;

namespace Assignment.ViewModels.Security
{
    /// <summary>
    /// View model for OTP verification during password reset.
    /// Users must enter the OTP code received via email before they can reset their password.
    /// Used by SecurityController.VerifyOtp action.
    /// </summary>
    public class VerifyOtpViewModel
    {
        /// <summary>
        /// The user's email address (hidden field, not displayed to user).
        /// Required field - used to identify which user account the OTP belongs to.
        /// This is passed from the forgot password form and used to verify the OTP code.
        /// </summary>
        [Required]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// The OTP (One-Time Password) code entered by the user.
        /// Must be exactly 6 digits (0-9).
        /// This code is sent to the user's email when they request password reset.
        /// OTP codes expire after 10 minutes for security.
        /// Required field - must match the OTP code stored in SecurityTokens table.
        /// </summary>
        [Required(ErrorMessage = "OTP code is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP code must be 6 digits")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP code must be 6 digits")]
        [Display(Name = "OTP Code")]
        public string OtpCode { get; set; } = string.Empty;
    }
}


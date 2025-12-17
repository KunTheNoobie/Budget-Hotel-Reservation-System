using System.ComponentModel.DataAnnotations;

namespace Assignment.ViewModels.Security
{
    /// <summary>
    /// View model for OTP verification during password reset.
    /// Users must enter the OTP code received via email before they can reset their password.
    /// </summary>
    public class VerifyOtpViewModel
    {
        /// <summary>
        /// The user's email address (hidden field).
        /// </summary>
        [Required]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// The OTP code entered by the user (6 digits).
        /// </summary>
        [Required(ErrorMessage = "OTP code is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP code must be 6 digits")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP code must be 6 digits")]
        [Display(Name = "OTP Code")]
        public string OtpCode { get; set; } = string.Empty;
    }
}


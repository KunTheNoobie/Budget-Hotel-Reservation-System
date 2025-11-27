using System.ComponentModel.DataAnnotations;
using Assignment.Models;

namespace Assignment.ViewModels.Security
{
    /// <summary>
    /// View model for the user registration form.
    /// Contains validation for user registration including password strength requirements
    /// and a math captcha for spam prevention.
    /// </summary>
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "Password must be 8-100 characters", MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", 
            ErrorMessage = "Password must contain uppercase, lowercase, number, and special character (@$!%*?&)")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [Display(Name = "Phone Number (Optional)")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Please answer the security question")]
        [Display(Name = "Security Question")]
        public int CaptchaAnswer { get; set; }
    }
}


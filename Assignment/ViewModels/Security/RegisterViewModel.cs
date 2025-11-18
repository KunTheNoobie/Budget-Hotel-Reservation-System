using System.ComponentModel.DataAnnotations;
using Assignment.Models;

namespace Assignment.ViewModels.Security
{
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

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, ErrorMessage = "Password must be at least 8 characters", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$", 
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, and one number")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [Display(Name = "Phone Number (Optional)")]
        public string? PhoneNumber { get; set; }
    }
}


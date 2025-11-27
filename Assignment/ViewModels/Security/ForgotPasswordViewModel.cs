using System.ComponentModel.DataAnnotations;

namespace Assignment.ViewModels.Security
{
    /// <summary>
    /// View model for the forgot password form.
    /// Used when a user requests a password reset via email.
    /// </summary>
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;
    }
}


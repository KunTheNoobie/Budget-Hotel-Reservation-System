using System.ComponentModel.DataAnnotations;

namespace Assignment.ViewModels.Security
{
    /// <summary>
    /// View model for the user login form.
    /// Contains email, password, remember me option, and return URL for post-login redirection.
    /// Used by SecurityController.Login action for user authentication.
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// User's email address (used as username).
        /// Must be a valid email format.
        /// Required field - cannot be empty.
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User's password for authentication.
        /// Password is hashed using BCrypt before comparison.
        /// Required field - cannot be empty.
        /// Input type is Password (hidden in browser).
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Remember me option - if true, authentication cookie persists longer.
        /// When checked, user stays logged in for extended period.
        /// When unchecked, cookie expires when browser closes.
        /// </summary>
        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }

        /// <summary>
        /// Return URL for redirecting user after successful login.
        /// Used when user tries to access a protected page while not logged in.
        /// Example: User clicks "Book Now" → redirected to login → after login, redirected back to booking page.
        /// Optional - if null, user is redirected based on their role (Admin/Manager/Staff → Admin panel, Customer → Home).
        /// </summary>
        public string? ReturnUrl { get; set; }
    }
}


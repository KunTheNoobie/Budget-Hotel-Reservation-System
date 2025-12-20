using Assignment.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Assignment.Helpers
{
    /// <summary>
    /// Helper class for managing user authentication using cookie-based authentication.
    /// Provides methods for signing users in/out, retrieving user information from claims,
    /// and checking authentication status.
    /// </summary>
    public static class AuthenticationHelper
    {
        // ========== Constants for Claim Types ==========

        /// <summary>
        /// Cookie name used for authentication (for reference, not directly used).
        /// </summary>
        public const string CookieName = "HotelReservationAuth";

        /// <summary>
        /// Claim type for storing the user's ID.
        /// </summary>
        public const string ClaimUserId = "UserId";

        /// <summary>
        /// Claim type for storing the user's email address.
        /// </summary>
        public const string ClaimUserEmail = "UserEmail";

        /// <summary>
        /// Claim type for storing the user's role (Admin, Manager, Staff, Customer).
        /// </summary>
        public const string ClaimUserRole = "UserRole";

        /// <summary>
        /// Claim type for storing the user's full name.
        /// </summary>
        public const string ClaimUserName = "UserName";

        /// <summary>
        /// Signs a user into the system by creating an authentication cookie with user claims.
        /// </summary>
        /// <param name="httpContext">The HTTP context for the current request.</param>
        /// <param name="user">The user to sign in.</param>
        /// <remarks>
        /// Creates a persistent cookie that expires after 7 days.
        /// Stores user ID, email, role, and name as claims for easy access throughout the application.
        /// </remarks>
        public static async Task SignInAsync(HttpContext httpContext, User user)
        {
            // ========== CREATE USER CLAIMS ==========
            // Create claims (user information) to store in the authentication cookie
            // Claims are key-value pairs that represent user information
            // These claims are stored in the cookie and can be accessed throughout the application
            var claims = new List<Claim>
            {
                // ========== CUSTOM CLAIMS ==========
                // Store user ID for easy access (e.g., for database queries)
                new Claim(ClaimUserId, user.UserId.ToString()),
                
                // Store user email for display and identification
                new Claim(ClaimUserEmail, user.Email),
                
                // Store user role for authorization checks (Admin, Manager, Staff, Customer)
                new Claim(ClaimUserRole, user.Role.ToString()),
                
                // Store user's full name for display in UI
                new Claim(ClaimUserName, user.FullName),
                
                // ========== STANDARD CLAIM TYPES ==========
                // Standard claim types for compatibility with ASP.NET Core Identity
                // These allow the application to work with standard authorization attributes
                new Claim(ClaimTypes.Name, user.Email),        // Standard name claim
                new Claim(ClaimTypes.Role, user.Role.ToString())  // Standard role claim
            };

            // ========== CREATE CLAIMS IDENTITY ==========
            // Create claims identity with cookie authentication scheme
            // ClaimsIdentity groups all the claims together and identifies the authentication method
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            
            // ========== CONFIGURE AUTHENTICATION PROPERTIES ==========
            // Configure authentication properties (persistent cookie, 7-day expiration)
            // These properties control how the authentication cookie behaves
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,                                    // Cookie persists across browser sessions
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)           // Cookie expires after 7 days
            };

            // ========== SIGN IN USER ==========
            // Sign in the user by creating the authentication cookie
            // This cookie is sent with every subsequent request to identify the user
            // The cookie is encrypted and signed to prevent tampering
            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,  // Use cookie authentication
                new ClaimsPrincipal(claimsIdentity),                // User's identity with claims
                authProperties);                                    // Cookie properties (expiration, etc.)
        }

        /// <summary>
        /// Signs the current user out of the system by removing the authentication cookie.
        /// </summary>
        /// <param name="httpContext">The HTTP context for the current request.</param>
        public static async Task SignOutAsync(HttpContext httpContext)
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Gets the user ID from the current user's authentication claims.
        /// </summary>
        /// <param name="httpContext">The HTTP context for the current request.</param>
        /// <returns>The user ID if authenticated, null otherwise.</returns>
        public static int? GetUserId(HttpContext httpContext)
        {
            // ========== GET USER ID FROM CLAIMS ==========
            // Find the UserId claim from the authentication cookie
            // Claims are stored in HttpContext.User after authentication
            var userIdClaim = httpContext.User.FindFirst(ClaimUserId);
            
            // ========== PARSE AND RETURN USER ID ==========
            // If claim exists and can be parsed as integer, return the user ID
            // Returns null if user is not authenticated or claim is missing
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }

        /// <summary>
        /// Gets the user role from the current user's authentication claims.
        /// </summary>
        /// <param name="httpContext">The HTTP context for the current request.</param>
        /// <returns>The user role if authenticated, null otherwise.</returns>
        public static UserRole? GetUserRole(HttpContext httpContext)
        {
            // ========== GET USER ROLE FROM CLAIMS ==========
            // Find the UserRole claim from the authentication cookie
            // Role is used for authorization (checking if user can access certain pages)
            var roleClaim = httpContext.User.FindFirst(ClaimUserRole);
            
            // ========== PARSE AND RETURN USER ROLE ==========
            // If claim exists and can be parsed as UserRole enum, return the role
            // Returns null if user is not authenticated or claim is missing
            // UserRole enum: Admin, Manager, Staff, Customer
            if (roleClaim != null && Enum.TryParse<UserRole>(roleClaim.Value, out UserRole role))
            {
                return role;
            }
            return null;
        }

        /// <summary>
        /// Gets the user email from the current user's authentication claims.
        /// </summary>
        /// <param name="httpContext">The HTTP context for the current request.</param>
        /// <returns>The user email if authenticated, null otherwise.</returns>
        public static string? GetUserEmail(HttpContext httpContext)
        {
            return httpContext.User.FindFirst(ClaimUserEmail)?.Value;
        }

        /// <summary>
        /// Gets the user's full name from the current user's authentication claims.
        /// </summary>
        /// <param name="httpContext">The HTTP context for the current request.</param>
        /// <returns>The user's full name if authenticated, null otherwise.</returns>
        public static string? GetUserName(HttpContext httpContext)
        {
            return httpContext.User.FindFirst(ClaimUserName)?.Value;
        }

        /// <summary>
        /// Checks if the current user is authenticated.
        /// </summary>
        /// <param name="httpContext">The HTTP context for the current request.</param>
        /// <returns>True if the user is authenticated, false otherwise.</returns>
        public static bool IsAuthenticated(HttpContext httpContext)
        {
            return httpContext.User.Identity?.IsAuthenticated ?? false;
        }
    }
}


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
            // Create claims (user information) to store in the authentication cookie
            var claims = new List<Claim>
            {
                new Claim(ClaimUserId, user.UserId.ToString()),
                new Claim(ClaimUserEmail, user.Email),
                new Claim(ClaimUserRole, user.Role.ToString()),
                new Claim(ClaimUserName, user.FullName),
                // Standard claim types for compatibility with ASP.NET Core Identity
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            // Create claims identity with cookie authentication scheme
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            
            // Configure authentication properties (persistent cookie, 7-day expiration)
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7) // 7 days
            };

            // Sign in the user by creating the authentication cookie
            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
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
            var userIdClaim = httpContext.User.FindFirst(ClaimUserId);
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
            var roleClaim = httpContext.User.FindFirst(ClaimUserRole);
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


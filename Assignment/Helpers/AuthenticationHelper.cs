using Assignment.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Assignment.Helpers
{
    public static class AuthenticationHelper
    {
        public const string CookieName = "HotelReservationAuth";
        public const string ClaimUserId = "UserId";
        public const string ClaimUserEmail = "UserEmail";
        public const string ClaimUserRole = "UserRole";
        public const string ClaimUserName = "UserName";

        public static async Task SignInAsync(HttpContext httpContext, User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimUserId, user.UserId.ToString()),
                new Claim(ClaimUserEmail, user.Email),
                new Claim(ClaimUserRole, user.Role.ToString()),
                new Claim(ClaimUserName, user.FullName),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7) // 7 days
            };

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        public static async Task SignOutAsync(HttpContext httpContext)
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        public static int? GetUserId(HttpContext httpContext)
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimUserId);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }

        public static UserRole? GetUserRole(HttpContext httpContext)
        {
            var roleClaim = httpContext.User.FindFirst(ClaimUserRole);
            if (roleClaim != null && Enum.TryParse<UserRole>(roleClaim.Value, out UserRole role))
            {
                return role;
            }
            return null;
        }

        public static string? GetUserEmail(HttpContext httpContext)
        {
            return httpContext.User.FindFirst(ClaimUserEmail)?.Value;
        }

        public static string? GetUserName(HttpContext httpContext)
        {
            return httpContext.User.FindFirst(ClaimUserName)?.Value;
        }

        public static bool IsAuthenticated(HttpContext httpContext)
        {
            return httpContext.User.Identity?.IsAuthenticated ?? false;
        }
    }
}


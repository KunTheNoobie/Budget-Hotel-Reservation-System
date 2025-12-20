using Assignment.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Assignment.Attributes
{
    /// <summary>
    /// Authorization attribute that restricts access to controllers or actions based on user roles.
    /// Can be applied to controller classes or individual action methods.
    /// If the user is not authenticated, redirects to the login page.
    /// If the user's role is not in the allowed roles list, returns a 403 Forbidden response.
    /// </summary>
    /// <example>
    /// [AuthorizeRole(UserRole.Admin, UserRole.Manager)] // Only Admin and Manager can access
    /// public class AdminController : Controller { }
    /// </example>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeRoleAttribute : Attribute, IAuthorizationFilter
    {
        /// <summary>
        /// Array of user roles that are allowed to access the protected resource.
        /// </summary>
        private readonly UserRole[] _allowedRoles;

        /// <summary>
        /// Initializes a new instance of the AuthorizeRoleAttribute with the specified allowed roles.
        /// </summary>
        /// <param name="allowedRoles">One or more user roles that are allowed to access the resource.</param>
        public AuthorizeRoleAttribute(params UserRole[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        /// <summary>
        /// Called by the ASP.NET Core framework to perform authorization checks.
        /// This method is executed before the controller action runs.
        /// If authorization fails, the action is not executed.
        /// </summary>
        /// <param name="context">The authorization filter context containing request and user information.</param>
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // ========== STEP 1: CHECK AUTHENTICATION ==========
            // Check if user is authenticated (logged in)
            // If not authenticated, user cannot access the protected resource
            if (!Helpers.AuthenticationHelper.IsAuthenticated(context.HttpContext))
            {
                // ========== REDIRECT TO LOGIN ==========
                // Not authenticated - redirect to login page with return URL
                // Return URL allows user to be redirected back after successful login
                // Example: User tries to access /Admin/Users → redirected to /Security/Login?returnUrl=/Admin/Users
                // After login, user is redirected back to /Admin/Users
                context.Result = new RedirectToActionResult("Login", "Security", new { returnUrl = context.HttpContext.Request.Path });
                return;  // Stop execution - don't check roles if not authenticated
            }

            // ========== STEP 2: CHECK AUTHORIZATION (ROLE) ==========
            // User is authenticated, now check if they have the required role
            // Get user's role from authentication claims
            var userRole = Helpers.AuthenticationHelper.GetUserRole(context.HttpContext);
            
            // Check if user's role is in the allowed roles list
            // If role is null or not in allowed roles, deny access
            if (userRole == null || !_allowedRoles.Contains(userRole.Value))
            {
                // ========== RETURN 403 FORBIDDEN ==========
                // User is authenticated but doesn't have the required role
                // Return 403 Forbidden status code
                // Example: Customer tries to access Admin page → 403 Forbidden
                context.Result = new ForbidResult();
            }
            // If role check passes, allow the request to continue (action will execute)
        }
    }
}


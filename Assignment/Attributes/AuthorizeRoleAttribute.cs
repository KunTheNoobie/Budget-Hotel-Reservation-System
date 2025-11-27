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
        /// </summary>
        /// <param name="context">The authorization filter context containing request and user information.</param>
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Check if user is authenticated
            if (!Helpers.AuthenticationHelper.IsAuthenticated(context.HttpContext))
            {
                // Not authenticated - redirect to login page with return URL
                context.Result = new RedirectToActionResult("Login", "Security", new { returnUrl = context.HttpContext.Request.Path });
                return;
            }

            // Check if user's role is in the allowed roles list
            var userRole = Helpers.AuthenticationHelper.GetUserRole(context.HttpContext);
            if (userRole == null || !_allowedRoles.Contains(userRole.Value))
            {
                // User is authenticated but doesn't have the required role - return 403 Forbidden
                context.Result = new ForbidResult();
            }
        }
    }
}


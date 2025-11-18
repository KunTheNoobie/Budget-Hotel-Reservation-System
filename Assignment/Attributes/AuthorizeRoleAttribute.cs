using Assignment.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Assignment.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly UserRole[] _allowedRoles;

        public AuthorizeRoleAttribute(params UserRole[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!Helpers.AuthenticationHelper.IsAuthenticated(context.HttpContext))
            {
                context.Result = new RedirectToActionResult("Login", "Security", new { returnUrl = context.HttpContext.Request.Path });
                return;
            }

            var userRole = Helpers.AuthenticationHelper.GetUserRole(context.HttpContext);
            if (userRole == null || !_allowedRoles.Contains(userRole.Value))
            {
                context.Result = new ForbidResult();
            }
        }
    }
}


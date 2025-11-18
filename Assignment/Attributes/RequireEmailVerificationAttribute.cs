using Assignment.Models.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace Assignment.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireEmailVerificationAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!Helpers.AuthenticationHelper.IsAuthenticated(context.HttpContext))
            {
                context.Result = new RedirectToActionResult("Login", "Security", new { returnUrl = context.HttpContext.Request.Path });
                return;
            }

            var userId = Helpers.AuthenticationHelper.GetUserId(context.HttpContext);
            if (userId == null)
            {
                context.Result = new RedirectToActionResult("Login", "Security", null);
                return;
            }

            var dbContext = context.HttpContext.RequestServices.GetRequiredService<HotelDbContext>();
            var user = dbContext.Users.Find(userId.Value);

            if (user == null || !user.IsEmailVerified)
            {
                context.Result = new RedirectToActionResult("VerifyEmail", "Security", null);
            }
        }
    }
}


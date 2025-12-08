using Backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;


namespace Backend.Attributes
{
    public class RequireSubscriptionAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var userIdClaim = context.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            if (!int.TryParse(userIdClaim, out var userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
            var user = await db.Users.FindAsync(userId);

            // Check subscription
            if (user == null || !user.IsSubscribed ||
                (user.SubscriptionExpiresAt.HasValue && user.SubscriptionExpiresAt < DateTime.UtcNow))
            {
                context.Result = new ForbidResult();
                return;
            }

            await next(); // allow action to execute
        }
    }
}

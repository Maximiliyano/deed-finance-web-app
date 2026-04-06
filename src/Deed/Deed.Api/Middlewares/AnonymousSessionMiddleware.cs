using Deed.Application.Auth;

namespace Deed.Api.Middlewares;

internal sealed class AnonymousSessionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            if (!context.Request.Cookies.TryGetValue(AnonymousConstants.SessionCookieName, out var anonId)
                || string.IsNullOrEmpty(anonId))
            {
                anonId = $"anon:{Guid.NewGuid():N}";
                context.Response.Cookies.Append(AnonymousConstants.SessionCookieName, anonId, new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true
                });
            }

            context.Items[AnonymousConstants.HttpContextItemKey] = anonId;
        }

        await next(context);
    }
}

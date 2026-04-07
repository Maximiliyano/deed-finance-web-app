using Deed.Api.Middlewares;

namespace Deed.Api.Extensions;

internal static class MiddlewareExtensions
{
    internal static IApplicationBuilder UseRequestContextLogging(this IApplicationBuilder app)
    {
        app.UseMiddleware<RequestContextLoggingMiddleware>();
        return app;
    }

    internal static IApplicationBuilder UseAnonymousSession(this IApplicationBuilder app)
    {
        app.UseMiddleware<AnonymousSessionMiddleware>();
        return app;
    }
}

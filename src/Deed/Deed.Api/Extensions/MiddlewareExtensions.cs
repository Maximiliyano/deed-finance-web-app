using Deed.Api.Middlewares;

namespace Deed.Api.Extensions;

internal static class MiddlewareExtensions
{
    internal static IApplicationBuilder UseProxyKey(this IApplicationBuilder app)
    {
        app.UseMiddleware<ProxyKeyMiddleware>();
        return app;
    }

    internal static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        app.UseMiddleware<SecurityHeadersMiddleware>();
        return app;
    }

    internal static IApplicationBuilder UseRequestContextLogging(this IApplicationBuilder app)
    {
        app.UseMiddleware<RequestContextLoggingMiddleware>();
        return app;
    }
}

using Microsoft.Net.Http.Headers;

namespace Deed.Api.Middlewares;

internal sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    public Task Invoke(HttpContext context)
    {
        context.Response.OnStarting(static state =>
        {
            IHeaderDictionary headers = ((HttpContext)state).Response.Headers;

            headers["X-Content-Type-Options"] = "nosniff";
            headers["X-Frame-Options"] = "DENY";
            headers["Referrer-Policy"] = "no-referrer";
            headers["Permissions-Policy"] = "geolocation=(), camera=(), microphone=()";
            headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";

            if (!headers.ContainsKey(HeaderNames.CacheControl))
            {
                headers[HeaderNames.CacheControl] = "no-store";
            }

            return Task.CompletedTask;
        }, context);

        return next(context);
    }
}

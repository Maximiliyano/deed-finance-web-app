using Microsoft.Extensions.Primitives;
using Serilog.Context;

namespace Deed.Api.Middlewares;

internal sealed class RequestContextLoggingMiddleware(RequestDelegate next)
{
    private const string CorrelationIdHeaderName = "CorrelationId";

    public Task Invoke(HttpContext context)
    {
        using (LogContext.PushProperty(CorrelationIdHeaderName, GetCorrelationId(context)))
        {
            return next.Invoke(context);
        }
    }

    private static string GetCorrelationId(HttpContext context)
    {
        context.Request.Headers.TryGetValue(
            CorrelationIdHeaderName,
            out StringValues correlationId);

        return correlationId.FirstOrDefault() ?? context.TraceIdentifier;
    }
}

using System.Security.Cryptography;
using System.Text;
using Deed.Application.Abstractions.Settings;
using Microsoft.Extensions.Options;

namespace Deed.Api.Middlewares;

internal sealed class ProxyKeyMiddleware(RequestDelegate next, IOptions<ProxySettings> options)
{
    private const string LivenessPath = "/health/live";

    private readonly byte[]? _key = string.IsNullOrEmpty(options.Value.Key)
        ? null
        : Encoding.UTF8.GetBytes(options.Value.Key);

    public Task Invoke(HttpContext context)
    {
        if (_key is null || IsLivenessProbe(context) || HasValidKey(context))
        {
            return next(context);
        }

        context.Response.StatusCode = StatusCodes.Status403Forbidden;

        return Task.CompletedTask;
    }

    private static bool IsLivenessProbe(HttpContext context)
        => context.Request.Path.Equals(LivenessPath, StringComparison.OrdinalIgnoreCase);

    private bool HasValidKey(HttpContext context)
    {
        string provided = context.Request.Headers[ProxyHeaders.ProxyKey].ToString();

        return provided.Length > 0
               && CryptographicOperations.FixedTimeEquals(_key, Encoding.UTF8.GetBytes(provided));
    }
}

using System.Globalization;
using System.Threading.RateLimiting;
using Deed.Api.Middlewares;
using Deed.Application.Abstractions.Settings;
using Deed.Application.Auth;
using Deed.Domain.Errors;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace Deed.Api.Extensions;

internal static class RateLimitingExtensions
{
    internal static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(_ => { });

        services.AddOptions<RateLimiterOptions>()
            .Configure<IOptions<RateLimitSettings>>((options, settings) =>
            {
                RateLimitSettings limits = settings.Value;

                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        $"api:{ResolveKey(context)}",
                        _ => PerMinute(limits.ApiPermitsPerMinute)));

                options.AddPolicy(RateLimitPolicies.Bank, context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        $"bank:{ResolveKey(context)}",
                        _ => PerMinute(limits.BankPermitsPerMinute)));

                options.AddPolicy(RateLimitPolicies.Auth, context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        $"auth:{ResolveKey(context)}",
                        _ => PerMinute(limits.AuthPermitsPerMinute)));

                options.OnRejected = async (context, _) =>
                {
                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
                    {
                        context.HttpContext.Response.Headers.RetryAfter =
                            ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);
                    }

                    await ResultExtensions
                        .ToProblemDetails([DomainErrors.General.TooManyRequests])
                        .ExecuteAsync(context.HttpContext);
                };
            });

        return services;
    }

    private static FixedWindowRateLimiterOptions PerMinute(int permits)
        => new()
        {
            PermitLimit = permits,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        };

    private static string ResolveKey(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            string? id = context.User.FindFirst(AuthClaimTypes.Sub)?.Value;

            if (!string.IsNullOrEmpty(id))
            {
                return $"user:{id}";
            }
        }

        return $"ip:{ResolveClientAddress(context)}";
    }

    private static string ResolveClientAddress(HttpContext context)
    {
        string? key = context.RequestServices.GetRequiredService<IOptions<ProxySettings>>().Value.Key;

        if (!string.IsNullOrEmpty(key))
        {
            string forwarded = context.Request.Headers[ProxyHeaders.ClientIp].ToString();

            if (forwarded.Length > 0)
            {
                return forwarded;
            }
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}

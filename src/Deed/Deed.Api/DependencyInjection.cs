using Deed.Api.Middlewares;
using Deed.Domain.Providers;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Deed.Api;

internal static class DependencyInjection
{
    internal static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddHttpContextAccessor();

        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.AddHealthChecks();

        services.AddCors();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        services.AddMemoryCache();

        return services;
    }
}

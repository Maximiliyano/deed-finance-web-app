using Deed.Api.Extensions;
using Deed.Api.Hubs;
using Deed.Api.Middlewares;
using Deed.Application.Abstractions.Services;
using Microsoft.AspNetCore.SignalR;
using Deed.Domain.Providers;

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

        services.AddSignalR();
        services.AddSingleton<IUserIdProvider, NameUserIdProvider>();
        services.AddSingleton<IBankNotificationPublisher, SignalRBankNotificationPublisher>();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        services.AddRateLimiting();

        return services;
    }
}

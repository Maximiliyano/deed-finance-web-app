using Deed.Application.Abstractions.Settings;
using Microsoft.Extensions.Options;

namespace Deed.Api.Extensions;

internal static class ApplicationBuilderExtensions
{
    internal static IApplicationBuilder UseSwaggerDependencies(this IApplicationBuilder builder)
        => builder
            .UseSwagger()
            .UseSwaggerUI();

    internal static IApplicationBuilder UseCorsPolicy(this IApplicationBuilder builder)
    {
        using var serviceScope = builder.ApplicationServices.CreateScope();

        var webUiSettings = serviceScope.ServiceProvider.GetRequiredService<IOptions<WebUrlSettings>>().Value;

        builder.UseCors(policyBuilder => policyBuilder
            .WithOrigins(webUiSettings.UIUrl)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
        );

        return builder;
    }
}

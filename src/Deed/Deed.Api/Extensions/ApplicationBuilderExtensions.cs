using Deed.Application.Abstractions.Settings;
using Microsoft.Extensions.Options;

namespace Deed.Api.Extensions;

internal static class ApplicationBuilderExtensions
{
    extension(IApplicationBuilder builder)
    {
        internal IApplicationBuilder UseSwaggerDependencies()
            => builder
                .UseSwagger()
                .UseSwaggerUI();

        internal IApplicationBuilder UseCorsPolicy()
        {
            using var serviceScope = builder.ApplicationServices.CreateScope();

            var webUiSettings = serviceScope.ServiceProvider.GetRequiredService<IOptions<WebUrlSettings>>().Value;

            builder.UseCors(policyBuilder => policyBuilder
                .WithOrigins(webUiSettings.UIUrl)
                .WithHeaders(webUiSettings.AllowedHeaders)
                .WithMethods(webUiSettings.AllowedMethods)
                .AllowCredentials()
            );

            return builder;
        }
    }
}

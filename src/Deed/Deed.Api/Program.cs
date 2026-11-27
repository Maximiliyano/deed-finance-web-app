using Deed.Api;
using Deed.Api.Extensions;
using Deed.Api.Hubs;
using Deed.Application;
using Deed.Application.Abstractions.Settings;
using Deed.Infrastructure;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilogDependencies();

builder.Configuration.AddEnvironmentVariables();

builder.Services
    .AddApplication(builder.Configuration)
    .AddApi()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddEndpoints();

var app = builder.Build();

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    await app.ApplyMigrationsAsync();
    app.UseSwaggerDependencies();
}
else
{
    app.UseHsts();
    app.UseSecurityHeaders();

    if (string.IsNullOrEmpty(app.Configuration[$"{nameof(ProxySettings)}:{nameof(ProxySettings.Key)}"]))
    {
        Log.Warning("ProxySettings:Key is not set, so requests are accepted without the proxy key");
    }
}

app.UseProxyKey();

app.UseExceptionHandler();

app.UseCorsPolicy();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

app.UseRequestContextLogging();

app.UseSerilogRequestLogging();

app.MapHealthChecks("health/live", new HealthCheckOptions
{
    Predicate = _ => false
}).AllowAnonymous();

app.MapHealthChecks("health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
}).AllowAnonymous();

app.MapEndpoints();

app.MapHub<BankNotificationsHub>(BankNotificationsHub.Route).RequireAuthorization();

await app.RunAsync();

namespace Deed.Api
{
    public partial class Program;
}

using Deed.Api;
using Deed.Api.Extensions;
using Deed.Application;
using Deed.Infrastructure;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilogDependencies();

builder.Configuration.AddEnvironmentVariables();

builder.Services
    .AddApplication(builder.Configuration, builder.Environment)
    .AddApi()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddEndpoints();

var app = builder.Build();

app.UseForwardedHeaders();

app.UseResponseCompression();

if (app.Environment.IsDevelopment())
{
    await app.ApplyMigrationsAsync();
    app.UseSwaggerDependencies();
}
else
{
    app.UseHsts();
}

app.UseExceptionHandler();

app.UseCorsPolicy();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseRequestContextLogging();

app.UseSerilogRequestLogging();

// Liveness — used by Fly's machine health check. No dependency probing, so a
// transient DB/Redis outage cannot mark the machine unhealthy and trigger a restart.
app.MapHealthChecks("health/live", new HealthCheckOptions
{
    Predicate = _ => false
}).AllowAnonymous();

// Readiness — full DB + Redis probe, for dashboards / manual checks.
app.MapHealthChecks("health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
}).AllowAnonymous();

app.MapEndpoints();

await app.RunAsync();

public partial class Program;

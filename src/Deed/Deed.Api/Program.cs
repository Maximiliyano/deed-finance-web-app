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

app.UseAuthentication();
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

await app.RunAsync();

namespace Deed.Api
{
    public partial class Program;
}

// TODO docker UI is cached and not rebuild like api
// TODO auth fails callback to the BE instaed redirect URI FE

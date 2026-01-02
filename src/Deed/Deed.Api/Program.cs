using System.Reflection;
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
    .AddApplication()
    .AddApi()
    .AddInfrastructure();

builder.Services.AddEndpoints();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // TODO: DB migrations run in Development without waiting for DB readiness. When using containers, the API may start before SQL Server is ready => migration/connection failures. Add a retry/healthy-wait loop.
    app.ApplyMigrations();
    app.UseSwaggerDependencies();
}
else
{ 
    app.UseHsts();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseCorsPolicy();

app.UseAuthentication();
app.UseAuthorization();

app.UseRequestContextLogging();

app.UseSerilogRequestLogging();

app.MapHealthChecks("health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapEndpoints();

await app.RunAsync();

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
    .AddInfrastructure();

builder.Services.AddEndpoints();

var app = builder.Build();

app.UseForwardedHeaders();

app.Use(async (context, next) =>
{
    Log.Information("Scheme={Scheme} Host={Host} XFP={Proto}",
        context.Request.Scheme,
        context.Request.Host,
        context.Request.Headers["X-Forwarded-Proto"].ToString());
    await next();
});

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

app.MapHealthChecks("health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
}).AllowAnonymous();

app.MapEndpoints();

await app.RunAsync();

public partial class Program;

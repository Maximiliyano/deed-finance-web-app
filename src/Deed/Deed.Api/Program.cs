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

builder.Services.AddAntiforgery(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.HeaderName = "X-XSRF-TOKEN";
});

var app = builder.Build();

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

app.Use(async (ctx, next) =>
{
    // If the request has forwarded headers, trust them (it came through a proxy)
    if (ctx.Request.Headers.TryGetValue("x-forwarded-host", out Microsoft.Extensions.Primitives.StringValues value))
    {
        ctx.Request.Scheme = ctx.Request.Headers["x-forwarded-proto"].FirstOrDefault() ?? ctx.Request.Scheme;
        ctx.Request.Host = new HostString(value.FirstOrDefault() ?? ctx.Request.Host.ToString());
    }
    await next();
});


app.Use(async (ctx, next) =>
{
    var h = ctx.Response.Headers;
    h["X-Content-Type-Options"] = "nosniff";
    h["X-Frame-Options"] = "DENY";
    h["Referrer-Policy"] = "no-referrer";
    h["Cache-Control"] = "no-store";
    await next();
});

app.UseCorsPolicy();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

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

using Deed.Api;
using Deed.Api.Extensions;
using Deed.Application;
using Deed.Infrastructure;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using AssemblyReference = Deed.Api.AssemblyReference;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilogDependencies();

builder.Configuration.AddEnvironmentVariables();

builder.Services
    .AddApplication()
    .AddApi()
    .AddInfrastructure();

builder.Services.AddEndpoints(AssemblyReference.Assembly);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
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

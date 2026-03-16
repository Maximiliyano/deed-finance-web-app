using System.Security.Claims;
using System.Text.Encodings.Web;
using Deed.Application.Abstractions.Data;
using Deed.Domain.Repositories;
using Deed.Infrastructure.Persistence;
using Deed.Infrastructure.Persistence.Interceptors;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Deed.Tests.Integration.Infrastructure;

public sealed class DeedApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string TestUser = "integration-test-user";
    private const string TestDbName = "DeedDB_IntegrationTests";

    private string _connectionString = string.Empty;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            string testDir = Path.GetDirectoryName(typeof(DeedApiFactory).Assembly.Location)!;
            config.SetBasePath(testDir)
                .AddJsonFile("appsettings.Testing.json", false);
        });

        builder.ConfigureTestServices(services =>
        {
            // Read base connection from config to get the server address
            ServiceProvider sp = services.BuildServiceProvider();
            IConfiguration config = sp.GetRequiredService<IConfiguration>();
            string baseConn = config.GetValue<string>("DatabaseConnection") ?? "";

            // Build test DB connection string from the base one
            _connectionString = ReplaceDatabase(baseConn, TestDbName);

            // Remove existing DB registrations
            List<ServiceDescriptor> toRemove = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<DeedDbContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType.FullName?.Contains("IDeedDbContext") == true ||
                d.ServiceType.FullName?.Contains("IUnitOfWork") == true ||
                d.ServiceType.FullName?.Contains("HealthCheck") == true ||
                d.ImplementationType?.FullName?.Contains("HealthCheck") == true ||
                d.ServiceType.FullName?.Contains("Quartz") == true ||
                d.ImplementationType?.FullName?.Contains("Quartz") == true
            ).ToList();
            foreach (ServiceDescriptor d in toRemove)
            {
                services.Remove(d);
            }

            // Register real SQL Server with test database
            services.AddDbContext<DeedDbContext>((svc, options) =>
            {
                UpdateAuditableEntitiesInterceptor interceptor =
                    svc.GetRequiredService<UpdateAuditableEntitiesInterceptor>();
                options.UseSqlServer(_connectionString)
                    .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
                    .AddInterceptors(interceptor);
            });

            services.AddScoped<IDeedDbContext>(svc => svc.GetRequiredService<DeedDbContext>());
            services.AddScoped<IUnitOfWork>(svc => svc.GetRequiredService<DeedDbContext>());

            services.AddHealthChecks();

            // Fake authentication
            services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = "Test";
                o.DefaultChallengeScheme = "Test";
                o.DefaultScheme = "Test";
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
        });
    }

    public async Task InitializeAsync()
    {
        // Create / migrate the test database
        using IServiceScope scope = Services.CreateScope();
        DeedDbContext db = scope.ServiceProvider.GetRequiredService<DeedDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        // Drop the test database after all tests
        using IServiceScope scope = Services.CreateScope();
        DeedDbContext db = scope.ServiceProvider.GetRequiredService<DeedDbContext>();
        await db.Database.EnsureDeletedAsync();
        await base.DisposeAsync();
    }

    private static string ReplaceDatabase(string connectionString, string newDb)
    {
        IEnumerable<string> parts = connectionString.Split(';')
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrEmpty(p))
            .Select(p => p.StartsWith("Database", StringComparison.OrdinalIgnoreCase)
                ? $"Database={newDb}"
                : p);
        return string.Join("; ", parts);
    }
}

internal sealed class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        Claim[] claims = new[]
        {
            new Claim("name", DeedApiFactory.TestUser),
            new Claim(ClaimTypes.NameIdentifier, DeedApiFactory.TestUser),
            new Claim(ClaimTypes.Email, "test@deed.finance"),
            new Claim("email_verified", "true")
        };

        ClaimsIdentity identity = new(claims, "Test");
        ClaimsPrincipal principal = new(identity);
        AuthenticationTicket ticket = new(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

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
            ServiceProvider sp = services.BuildServiceProvider();
            IConfiguration config = sp.GetRequiredService<IConfiguration>();
            string baseConn = config.GetValue<string>("DatabaseConnection") ?? "";

            _connectionString = ReplaceDatabase(baseConn, TestDbName);

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
        using IServiceScope scope = Services.CreateScope();
        DeedDbContext db = scope.ServiceProvider.GetRequiredService<DeedDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
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

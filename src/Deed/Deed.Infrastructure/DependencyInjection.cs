using Deed.Application.Abstractions.Caching;
using Deed.Application.Abstractions.Data;
using Deed.Application.Abstractions.Services;
using Deed.Application.Abstractions.Settings;
using Deed.Application.BankConnections.Service;
using Deed.Domain.Repositories;
using Deed.Infrastructure.BackgroundJobs.BankAccountRefresh;
using Deed.Infrastructure.BackgroundJobs.BankBackfill;
using Deed.Infrastructure.BackgroundJobs.BankTransactionSync;
using Deed.Infrastructure.BackgroundJobs.ReminderDigest;
using Deed.Infrastructure.BackgroundJobs.StatementMailSync;
using Deed.Infrastructure.BackgroundJobs.UpsertLatestExchange;
using Deed.Infrastructure.Caching;
using Deed.Infrastructure.Persistence;
using Deed.Infrastructure.Persistence.Constants;
using Deed.Infrastructure.Persistence.Interceptors;
using Deed.Infrastructure.Persistence.Repositories;
using Deed.Infrastructure.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System.Security.Cryptography.X509Certificates;

namespace Deed.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbDependencies();

        services.AddRedisCaching(configuration);

        services.AddKeyRingProtection(configuration);

        services.AddRepositories();

        services.AddBackgroundJobs();

        services.AddScoped<IEmailService, SmtpEmailService>();

        services.AddSingleton<ITokenProtector, DataProtectionTokenProtector>();

        return services;
    }

    private static IServiceCollection AddKeyRingProtection(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(nameof(DataProtectionSettings));
        var settings = section.Get<DataProtectionSettings>() ?? new DataProtectionSettings();

        var builder = services.AddDataProtection()
            .SetApplicationName("Deed")
            .PersistKeysToDbContext<DeedDbContext>();

        if (settings.HasCertificate)
        {
            var certificate = X509CertificateLoader.LoadPkcs12(Convert.FromBase64String(settings.CertificateBase64!), settings.CertificatePassword);

            builder.ProtectKeysWithCertificate(certificate).UnprotectKeysWithAnyCertificate(certificate);
        }

        services.Configure<DataProtectionSettings>(section);

        services.AddHostedService<KeyRingProtectionService>();

        return services;
    }

    private static IServiceCollection AddRedisCaching(this IServiceCollection services, IConfiguration configuration)
    {
        string redisConnection = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("Redis connection string is not configured.");

        string redisConfig = ParseRedisConnection(redisConnection);

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConfig;
            options.InstanceName = "deed:";
        });

        services.AddHealthChecks()
            .AddRedis(redisConfig, name: "deed_redis", tags: ["cache", "redis"]);

        services.AddSingleton<ICacheService, RedisCacheService>();

        return services;
    }

    private static IServiceCollection AddBackgroundJobs(this IServiceCollection services)
    {
        services.AddQuartz();

        services.AddQuartzHostedService(options
            => options.WaitForJobsToComplete = true);

        services.ConfigureOptions<UpsertLatestExchangeJobSetup>();

        services.ConfigureOptions<ReminderDigestJobSetup>();

        services.ConfigureOptions<BankTransactionSyncJobSetup>();

        services.AddSingleton<IBankBackfillTrigger, QuartzBankBackfillTrigger>();

        services.ConfigureOptions<BankBackfillJobSetup>();

        services.ConfigureOptions<BankAccountRefreshJobSetup>();

        services.ConfigureOptions<StatementMailSyncJobSetup>();

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<ICapitalRepository, CapitalRepository>();

        services.AddScoped<ICategoryRepository, CategoryRepository>();

        services.AddScoped<IExpenseRepository, ExpenseRepository>();

        services.AddScoped<IExchangeRepository, ExchangeRepository>();

        services.AddScoped<IIncomeRepository, IncomeRepository>();

        services.AddScoped<ITagRepository, TagRepository>();

        services.AddScoped<IExpenseTagRepository, ExpenseTagRepository>();

        services.AddScoped<IUserSettingsRepository, UserSettingsRepository>();

        services.AddScoped<IBudgetEstimationRepository, BudgetEstimationRepository>();

        services.AddScoped<IGoalRepository, GoalRepository>();

        services.AddScoped<IDebtRepository, DebtRepository>();

        services.AddScoped<ITransferRepository, TransferRepository>();

        services.AddScoped<IUtilityBillRepository, UtilityBillRepository>();

        services.AddScoped<IBankConnectionRepository, BankConnectionRepository>();

        services.AddScoped<IBankAccountRepository, BankAccountRepository>();

        services.AddScoped<IBankNotificationRepository, BankNotificationRepository>();

        return services;
    }

    private static string ParseRedisConnection(string connection)
    {
        if (!Uri.TryCreate(connection, UriKind.Absolute, out var uri) ||
            uri.Scheme is not ("redis" or "rediss"))
        {
            return connection;
        }

        int port = uri.Port > 0 ? uri.Port : 6379;
        string config = $"{uri.Host}:{port}";

        string password = uri.UserInfo.Contains(':')
            ? uri.UserInfo.Split(':', 2)[1]
            : uri.UserInfo;

        if (!string.IsNullOrEmpty(password))
        {
            config += $",password={password}";
        }

        if (uri.Scheme == "rediss")
        {
            config += ",ssl=true";
        }

        return config;
    }

    private static IServiceCollection AddDbDependencies(this IServiceCollection services)
    {
        services.AddSingleton<UpdateAuditableEntitiesInterceptor>();

        services.AddDbContextPool<DeedDbContext>((sp, options) =>
        {
            var databaseSettings = sp.GetRequiredService<IConfiguration>().GetValue<string>(ConfigurationKeys.DatabaseConnection);
            var auditableInterceptor = sp.GetRequiredService<UpdateAuditableEntitiesInterceptor>();

            options.UseSqlServer(databaseSettings, sql => sql
                    .EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null)
                    .CommandTimeout(30))
                .ConfigureWarnings(w =>
                    w.Ignore(RelationalEventId.PendingModelChangesWarning))
                .AddInterceptors(auditableInterceptor);
        });

        services.AddHealthChecks()
            .AddDbContextCheck<DeedDbContext>(name: "deed_db", tags: ["db", "sql"]);

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<DeedDbContext>());
        services.AddScoped<IDeedDbContext>(sp => sp.GetRequiredService<DeedDbContext>());

        return services;
    }
}

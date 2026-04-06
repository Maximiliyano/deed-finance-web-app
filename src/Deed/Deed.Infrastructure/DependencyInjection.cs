using Deed.Application.Abstractions.Caching;
using Deed.Application.Abstractions.Data;
using Deed.Application.Abstractions.Services;
using Deed.Domain.Repositories;
using Deed.Infrastructure.BackgroundJobs.BalanceReminder;
using Deed.Infrastructure.BackgroundJobs.DebtReminder;
using Deed.Infrastructure.BackgroundJobs.ExpenseReminder;
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

namespace Deed.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbDependencies();

        services.AddRedisCaching(configuration);

        services.AddDataProtection()
            .SetApplicationName("Deed")
            .PersistKeysToDbContext<DeedDbContext>();

        services.AddRepositories();

        services.AddBackgroundJobs();

        services.AddScoped<IEmailService, SmtpEmailService>();

        return services;
    }

    private static IServiceCollection AddRedisCaching(this IServiceCollection services, IConfiguration configuration)
    {
        string redisConnection = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("Redis connection string is not configured.");

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
            options.InstanceName = "deed:";
        });

        services.AddHealthChecks()
            .AddRedis(redisConnection, name: "deed_redis", tags: ["cache", "redis"]);

        services.AddSingleton<ICacheService, RedisCacheService>();

        return services;
    }

    private static IServiceCollection AddBackgroundJobs(this IServiceCollection services)
    {
        services.AddQuartz();

        services.AddQuartzHostedService(options
            => options.WaitForJobsToComplete = true);

        services.ConfigureOptions<UpsertLatestExchangeJobSetup>();

        services.ConfigureOptions<BalanceReminderJobSetup>();

        services.ConfigureOptions<ExpenseReminderJobSetup>();

        services.ConfigureOptions<DebtReminderJobSetup>();

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

        return services;
    }

    private static IServiceCollection AddDbDependencies(this IServiceCollection services)
    {
        services.AddSingleton<UpdateAuditableEntitiesInterceptor>();

        services.AddDbContext<DeedDbContext>((sp, options) =>
        {
            var databaseSettings = sp.GetRequiredService<IConfiguration>().GetValue<string>(ConfigurationKeys.DatabaseConnection);
            var auditableInterceptor = sp.GetRequiredService<UpdateAuditableEntitiesInterceptor>();

            options.UseSqlServer(databaseSettings)
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

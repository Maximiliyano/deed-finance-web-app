using Deed.Application.Abstractions.Data;
using Deed.Domain.Repositories;
using Deed.Infrastructure.BackgroundJobs.UpsertLatestExchange;
using Deed.Infrastructure.Persistence;
using Deed.Infrastructure.Persistence.Constants;
using Deed.Infrastructure.Persistence.Interceptors;
using Deed.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Deed.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddDbDependencies();

        services.AddRepositories();

        services.AddBackgroundJobs();

        return services;
    }

    private static IServiceCollection AddBackgroundJobs(this IServiceCollection services)
    {
        services.AddQuartz();

        services.AddQuartzHostedService(options
            => options.WaitForJobsToComplete = true);

        services.ConfigureOptions<UpsertLatestExchangeJobSetup>();

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

        return services;
    }

    private static IServiceCollection AddDbDependencies(this IServiceCollection services)
    {
        services.AddSingleton<UpdateAuditableEntitiesInterceptor>();

        services.AddDbContext<DeedDbContext>((sp, options) =>
        {
            var databaseSettings = sp.GetRequiredService<IConfiguration>().GetValue<string>(TableConfigurationConstants.DatabaseConnection);
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

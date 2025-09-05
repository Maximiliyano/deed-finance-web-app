using Deed.Application.Abstractions.Data;
using Deed.Domain.Repositories;
using Deed.Infrastructure.BackgroundJobs.SaveLatestExchange;
using Deed.Infrastructure.Persistence;
using Deed.Infrastructure.Persistence.Constants;
using Deed.Infrastructure.Persistence.DataSeed;
using Deed.Infrastructure.Persistence.Interceptors;
using Deed.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Deed.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddDbDependencies();

        services.AddIdentityDependencies();

        services.AddRepositories();

        services.AddBackgroundJobs();

        return services;
    }

    private static IServiceCollection AddIdentityDependencies(this IServiceCollection services)
    {
        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<DeedDbContext>();

        return services;
    }

    private static IServiceCollection AddBackgroundJobs(this IServiceCollection services)
    {
        services.AddQuartz();

        services.AddQuartzHostedService(options
            => options.WaitForJobsToComplete = true);

        services.ConfigureOptions<SaveLatestExchangeJobSetup>();

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddTransient<ICapitalRepository, CapitalRepository>();

        services.AddTransient<IExpenseRepository, ExpenseRepository>();

        services.AddTransient<IExchangeRepository, ExchangeRepository>();

        services.AddTransient<IIncomeRepository, IncomeRepository>();

        services.AddTransient<ICategoryRepository, CategoryRepository>();

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

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<DeedDbContext>());
        services.AddScoped<IDeedDbContext>(sp => sp.GetRequiredService<DeedDbContext>());

        return services;
    }
}

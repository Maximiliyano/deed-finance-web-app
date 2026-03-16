using Deed.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Deed.Api.Extensions;

internal static class MigrationExtensions
{
    public static async Task<IApplicationBuilder> ApplyMigrationsAsync(this IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.CreateScope();

        var dbContext = serviceScope.ServiceProvider.GetRequiredService<DeedDbContext>();

        await dbContext.Database.MigrateAsync();

        return app;
    }
}

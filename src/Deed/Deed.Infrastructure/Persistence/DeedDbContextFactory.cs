using Deed.Application.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace Deed.Infrastructure.Persistence;

internal sealed class DeedDbContextFactory(IDbContextFactory<DeedDbContext> factory) : IDeedDbContextFactory
{
    public IDeedDbContext CreateReadOnlyContext()
    {
        var context = factory.CreateDbContext();
        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        return context;
    }
}

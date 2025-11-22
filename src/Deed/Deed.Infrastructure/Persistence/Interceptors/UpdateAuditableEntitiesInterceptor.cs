using Deed.Domain.Entities;
using Deed.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Deed.Infrastructure.Persistence.Interceptors;

internal sealed class UpdateAuditableEntitiesInterceptor(IDateTimeProvider dateTimeProvider)
    : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            SoftDeleteAuditableEntities(eventData.Context);
            UpdateAuditableEntities(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void SoftDeleteAuditableEntities(DbContext context)
    {
        var entries = context
            .ChangeTracker
            .Entries<ISoftDeletableEntity>()
            .Where(e => e.State == EntityState.Deleted)
            .ToList();

        foreach (var entry in entries)
        {
            SetCurrentPropertyValue(entry, nameof(ISoftDeletableEntity.IsDeleted), true);
            entry.State = EntityState.Modified;
        }
    }

    private void UpdateAuditableEntities(DbContext context)
    {
        var entries = context
            .ChangeTracker
            .Entries<IAuditableEntity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .ToList();

        Parallel.ForEach(entries, entry =>
        {
            if (entry.State == EntityState.Added)
            {
                SetCurrentPropertyValue(entry, nameof(IAuditableEntity.CreatedBy), 0); // TODO userID
                SetCurrentPropertyValue(entry, nameof(IAuditableEntity.CreatedAt), dateTimeProvider.UtcNow);
            }
            else
            {
                SetCurrentPropertyValue(entry, nameof(IAuditableEntity.UpdatedBy), 0); // TODO userID
                SetCurrentPropertyValue(entry, nameof(IAuditableEntity.UpdatedAt), dateTimeProvider.UtcNow);
            }
        });
    }

    private static void SetCurrentPropertyValue(
        EntityEntry entry,
        string propertyName,
        object? value)
            => entry.Property(propertyName).CurrentValue = value;
}

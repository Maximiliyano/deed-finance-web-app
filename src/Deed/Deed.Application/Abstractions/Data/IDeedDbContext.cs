using Deed.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Deed.Application.Abstractions.Data;

public interface IDeedDbContext
{
    DbSet<TEntity> Set<TEntity>()
        where TEntity : Entity;

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}

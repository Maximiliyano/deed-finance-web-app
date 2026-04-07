using Deed.Domain.Entities;
using Deed.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Deed.Application.Abstractions.Data;

public interface IDeedDbContext : IDisposable
{
    DbSet<TEntity> Set<TEntity>()
        where TEntity : Entity;

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    Task<List<TEntity>> QueryAsync<TEntity>(ISpecification<TEntity> specification, CancellationToken ct = default)
        where TEntity : Entity;

    Task<TEntity?> QuerySingleAsync<TEntity>(ISpecification<TEntity> specification, CancellationToken ct = default)
        where TEntity : Entity;
}

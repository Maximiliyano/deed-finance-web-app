using Deed.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Deed.Application.Abstractions.Data;

public interface IDeedDbContext
{
    DbSet<Capital> Capitals { get; }

    DbSet<Category> Categories { get; }

    DbSet<Transfer> Transfers { get; }

    DbSet<Income> Incomes { get; }

    DbSet<Expense> Expenses { get; }

    DbSet<Exchange> Exchanges { get; }

    DbSet<TEntity> Set<TEntity>()
        where TEntity : Entity;

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}

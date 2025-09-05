using Deed.Application.Abstractions.Data;
using Deed.Domain.Entities;
using Deed.Domain.Repositories;
using Deed.Infrastructure.Persistence.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Deed.Infrastructure.Persistence.Repositories;

internal abstract class GeneralRepository<TEntity>(IDeedDbContext context)
    where TEntity : Entity, ISoftDeletableEntity
{
    protected IDeedDbContext DbContext { get; } = context;
    protected readonly DbSet<TEntity> DbSet = context.Set<TEntity>();

    protected async Task<IEnumerable<TEntity>> GetAllAsync() =>
        await DbSet
            .AsNoTracking()
            .ToListAsync();

    protected async Task<TEntity?> GetAsync(ISpecification<TEntity> specification) =>
        await ApplySpecification(specification)
            .SingleOrDefaultAsync();

    protected void Create(TEntity entity) =>
        DbSet.Add(entity);

    protected void CreateRange(IEnumerable<TEntity> entities) =>
        DbSet.AddRange(entities);

    protected void Update(TEntity entity) =>
        DbSet.Update(entity);

    protected void UpdateRange(IEnumerable<TEntity> entities) =>
        DbSet.UpdateRange(entities);

    protected void Delete(TEntity entity) =>
        DbSet.Remove(entity);

    protected void DeleteRange(IEnumerable<TEntity> entities) =>
        DbSet.RemoveRange(entities);

    protected async Task<bool> AnyAsync(ISpecification<TEntity> specification) =>
        await ApplySpecification(specification)
            .AnyAsync();

    private IQueryable<TEntity> ApplySpecification(ISpecification<TEntity>? specification)
        => SpecificationEvaluator.GetQuery(DbSet, specification);
}

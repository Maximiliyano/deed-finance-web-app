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

    public async Task<IEnumerable<TEntity>> GetAllAsync() =>
        await DbSet
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync();

    public async Task<IEnumerable<TEntity>> GetAllAsync(ISpecification<TEntity> specification) =>
        await ApplySpecification(specification)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync();

    public async Task<TEntity?> GetAsync(ISpecification<TEntity> specification) =>
        await ApplySpecification(specification)
            .SingleOrDefaultAsync();

    public void Create(TEntity entity) =>
        DbSet.Add(entity);

    public void CreateRange(IEnumerable<TEntity> entities) =>
        DbSet.AddRange(entities);

    public void Update(TEntity entity) =>
        DbSet.Update(entity);

    public void UpdateRange(IEnumerable<TEntity> entities) =>
        DbSet.UpdateRange(entities);

    public void Delete(TEntity entity) => 
        DbSet.Remove(entity);

    public async Task<bool> AnyAsync(ISpecification<TEntity> specification) =>
        await ApplySpecification(specification)
            .AnyAsync();

    private IQueryable<TEntity> ApplySpecification(ISpecification<TEntity>? specification)
        => SpecificationEvaluator.GetQuery(DbSet, specification);
}

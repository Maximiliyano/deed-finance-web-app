using Deed.Domain.Entities;
using Deed.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Deed.Infrastructure.Persistence.Abstractions;

internal static class SpecificationEvaluator
{
    public static IQueryable<TEntity> GetQuery<TEntity>(
        IQueryable<TEntity> queryable,
        ISpecification<TEntity>? specification = null)
            where TEntity : Entity
    {
        if (specification is null)
        {
            return queryable;
        }

        if (specification.Tracking != true)
        {
            queryable = queryable.AsNoTracking();
        }

        if (specification.IgnoreQueryFilter == true)
        {
            queryable = queryable.IgnoreQueryFilters();
        }

        if (specification.Criteria is not null)
        {
            queryable = queryable.Where(specification.Criteria);
        }

        if (specification.Includes.Count > 0)
        {
            queryable = specification.Includes
                .Aggregate(
                    queryable,
                    (current, includeExpression) =>
                        includeExpression(current))
                .AsSplitQuery();
        }

        if (specification.OrderBy is not null)
        {
            queryable = queryable.OrderBy(specification.OrderBy);
        }

        if (specification.OrderByDescending is not null)
        {
            queryable = queryable.OrderByDescending(specification.OrderByDescending);
        }

        if (specification.Skip.HasValue)
        {
            queryable = queryable.Skip(specification.Skip.Value);
        }

        if (specification.Take.HasValue)
        {
            queryable = queryable.Take(specification.Take.Value);
        }

        return queryable;
    }
}

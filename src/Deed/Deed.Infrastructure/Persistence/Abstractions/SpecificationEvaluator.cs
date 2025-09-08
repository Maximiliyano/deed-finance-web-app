using System.Globalization;
using Deed.Domain.Constants;
using Deed.Domain.Entities;
using Deed.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Deed.Infrastructure.Persistence.Abstractions;

public static class SpecificationEvaluator
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

        if (specification.Criteria is not null)
        {
            queryable = queryable.Where(specification.Criteria);
        }

        if (specification.Includes is not null && !specification.IgnoreAutoIncludes)
        {
            queryable = specification.Includes
                .Aggregate(
                    queryable,
                    (currect, includeExpression) =>
                        currect.Include(includeExpression))
                .AsSplitQuery();
        }

        if (specification.IgnoreAutoIncludes)
        {
            queryable = queryable.IgnoreAutoIncludes();
        }

        if (specification.OrderBy is not null)
        {
            queryable = queryable.OrderBy(specification.OrderBy);
        }

        if (specification.OrderByDescending is not null)
        {
            queryable = queryable.OrderByDescending(specification.OrderByDescending);
        }

        return queryable;
    }
}

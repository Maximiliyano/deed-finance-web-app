using System.Linq.Expressions;

namespace Deed.Domain.Repositories;

public interface ISpecification<TEntity>
{
    Expression<Func<TEntity, bool>>? Criteria { get; }

    List<Func<IQueryable<TEntity>, IQueryable<TEntity>>> Includes { get; }

    Expression<Func<TEntity, object>>? OrderBy { get; }

    Expression<Func<TEntity, object>>? OrderByDescending { get; }

    bool? IgnoreQueryFilter { get; }

    bool? Tracking { get; }
}

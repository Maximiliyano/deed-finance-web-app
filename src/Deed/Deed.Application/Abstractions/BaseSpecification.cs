using System.Linq.Expressions;
using Deed.Domain.Entities;
using Deed.Domain.Repositories;

namespace Deed.Application.Abstractions;

internal abstract class BaseSpecification<TEntity>(
    Expression<Func<TEntity, bool>>? criteria = null,
    bool ignoreAutoIncludes = false)
    : ISpecification<TEntity>
    where TEntity : Entity
{
    public bool IgnoreAutoIncludes { get; protected init; } = ignoreAutoIncludes;

    public Expression<Func<TEntity, object>>? OrderBy { get; protected set; }

    public Expression<Func<TEntity, object>>? OrderByDescending { get; protected set; }

    public Expression<Func<TEntity, bool>>? Criteria { get; protected init; } = criteria;

    public IList<Expression<Func<TEntity, object>>> Includes { get; } = [];

    protected void AddInclude(Expression<Func<TEntity, object>> include)
    {
        if (!IgnoreAutoIncludes)
        {
            Includes.Add(include);
        }
    }

    protected void ApplyOrderByDescending(Expression<Func<TEntity, object>> expression)
    {
        OrderByDescending = expression;
    }

    protected void ApplyOrderBy(Expression<Func<TEntity, object>> expression)
    {
        OrderBy = expression;
    }
}

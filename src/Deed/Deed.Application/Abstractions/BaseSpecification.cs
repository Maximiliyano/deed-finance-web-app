using System.Linq.Expressions;
using Deed.Domain.Entities;
using Deed.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Deed.Application.Abstractions;

internal abstract class BaseSpecification<TEntity>(
    Expression<Func<TEntity, bool>>? criteria = null)
    : ISpecification<TEntity>
    where TEntity : Entity
{
    public Expression<Func<TEntity, object>>? OrderBy { get; protected set; }

    public Expression<Func<TEntity, object>>? OrderByDescending { get; protected set; }

    public Expression<Func<TEntity, bool>>? Criteria { get; protected init; } = criteria;

    public List<Func<IQueryable<TEntity>, IQueryable<TEntity>>> Includes { get; } = [];

    public bool? IgnoreQueryFilter { get; protected set; }

    public bool? Tracking { get; protected set; }

    protected void AddInclude(Expression<Func<TEntity, object>> expression)
    {
        Includes.Add(i => i.Include(expression));
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

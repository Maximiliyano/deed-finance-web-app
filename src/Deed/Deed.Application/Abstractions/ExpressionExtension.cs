using System.Linq.Expressions;
using Deed.Domain.Entities;

namespace Deed.Application.Abstractions;

internal static class ExpressionExtension
{
    public static Expression<Func<Capital, bool>> CombineExpressions(
        Expression<Func<Capital, bool>> expr1,
        Expression<Func<Capital, bool>> expr2)
    {
        var param = Expression.Parameter(typeof(Capital));

        var combined = Expression.Lambda<Func<Capital, bool>>(
            Expression.AndAlso(
                Expression.Invoke(expr1, param),
                Expression.Invoke(expr2, param)
            ),
            param
        );

        return combined;
    }
}

using System.Linq.Expressions;

namespace Deed.Application.Abstractions;

internal static class ExpressionExtension
{
    public static Expression<Func<T, bool>> Combine<T>(
        Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2)
    {
        var param = expr1.Parameters[0];
        var body = Expression.AndAlso(
            expr1.Body,
            new ParameterReplacer(expr2.Parameters[0], param).Visit(expr2.Body));
        return Expression.Lambda<Func<T, bool>>(body, param);
    }

    private sealed class ParameterReplacer(ParameterExpression from, ParameterExpression to) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
            => node == from ? to : base.VisitParameter(node);
    }
}

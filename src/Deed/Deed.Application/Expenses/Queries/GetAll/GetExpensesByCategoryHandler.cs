using Deed.Application.Abstractions.Messaging;
using Deed.Application.Categories.Response;
using Deed.Application.Expenses.Responses;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.Expenses.Queries.GetAll;

internal sealed class GetExpensesByCategoryHandler(
    IExpenseRepository repository)
    : IQueryHandler<GetExpensesByCategoryQuery, IEnumerable<CategoryExpenseResponse>>
{
    public async Task<Result<IEnumerable<CategoryExpenseResponse>>> Handle(GetExpensesByCategoryQuery query, CancellationToken cancellationToken)
    {
        var expenses = await repository.GetAllAsync(query.CategoryId).ConfigureAwait(false);

        if (!expenses.Any())
        {
            return Result.Success(Enumerable.Empty<CategoryExpenseResponse>());
        }

        var totalSum = expenses.Sum(e => e.Amount);
        
        const decimal Epsilon = 0.0001m;

        var grouped = expenses
            .GroupBy(e => e.CategoryId)
            .Select(g =>
            {
                var categorySum = g.Sum(e => e.Amount);
                var category = g.First().Category;

                var percentage = Math.Abs(totalSum) < Epsilon
                    ? 0m
                    : Math.Round(categorySum / totalSum * 100, 2);

                return new CategoryExpenseResponse(
                    g.Key,
                    category?.Name ?? "Undefined",
                    categorySum,
                    percentage,
                    category?.PlannedPeriodAmount ?? 0m,
                    category?.Period.ToString() ?? nameof(PerPeriodType.None),
                    g.ToResponses()
                );
                
            });

        return Result.Success(grouped);
    }
}

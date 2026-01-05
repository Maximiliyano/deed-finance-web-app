using Deed.Application.Expenses.Commands.Create;
using Deed.Application.Expenses.Responses;
using Deed.Application.Capitals;
using Deed.Application.Categories;
using Deed.Domain.Entities;

namespace Deed.Application.Expenses;

internal static class ExpenseExtensions
{
    internal static ExpenseResponse ToResponse(this Expense expense)
        => new(
            expense.Id,
            expense.CategoryId,
            expense.CapitalId,
            expense.Amount,
            expense.PaymentDate,
            expense.Purpose,
            [.. expense.Tags.Select(t => t.Tag.Name)]
        );

    internal static IEnumerable<ExpenseResponse> ToResponses(this IEnumerable<Expense> expenses)
        => expenses.Select(e => e.ToResponse());

    internal static Expense ToEntity(this CreateExpenseCommand command)
        => new()
        {
            Amount = command.Amount,
            Purpose = command.Purpose,
            CategoryId = command.CategoryId,
            CapitalId = command.CapitalId,
            PaymentDate = command.PaymentDate
        };
}

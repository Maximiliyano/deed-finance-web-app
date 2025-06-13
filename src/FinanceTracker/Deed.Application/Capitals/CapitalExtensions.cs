using FinanceTracker.Application.Capitals.Commands.Create;
using FinanceTracker.Application.Capitals.Responses;
using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Application.Capitals;

internal static class CapitalExtensions
{
    internal static CapitalResponse ToResponse(this Capital capital)
        => new(
            capital.Id,
            capital.Name,
            capital.Balance,
            capital.Currency.ToString(),
            capital.IncludeInTotal,
            capital.TotalIncome,
            capital.TotalExpense,
            capital.TotalTransferIn,
            capital.TotalTransferOut);

    internal static IEnumerable<CapitalResponse> ToResponses(this IEnumerable<Capital> capitals)
        => capitals.Select(e => e.ToResponse());

    internal static Capital ToEntity(this CreateCapitalCommand command)
        => new()
        {
            Name = command.Name.Trim(),
            Balance = command.Balance,
            Currency = command.Currency
        };
}

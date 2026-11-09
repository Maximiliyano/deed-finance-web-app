using Deed.Application.Capitals.Commands.Create;
using Deed.Application.Capitals.Responses;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;

namespace Deed.Application.Capitals;

internal static class CapitalExtensions
{
    internal static CapitalResponse ToResponse(this CapitalSummary summary)
        => new(
            summary.Capital.Id,
            summary.Capital.Name,
            summary.Capital.Balance,
            summary.Capital.Currency.ToString(),
            summary.Capital.IncludeInTotal,
            summary.Capital.OnlyForSavings,
            summary.TotalIncome,
            summary.TotalExpense,
            summary.TotalTransferIn,
            summary.TotalTransferOut,
            summary.Capital.CreatedAt,
            summary.Capital.CreatedBy
    );

    internal static IEnumerable<CapitalResponse> ToResponses(this IEnumerable<CapitalSummary> summaries)
        => summaries.Select(e => e.ToResponse());

    internal static Capital ToEntity(this CreateCapitalCommand command)
        => new()
        {
            Name = command.Name.Trim(),
            Balance = command.Balance,
            Currency = Enum.Parse<CurrencyType>(command.Currency),
            IncludeInTotal = command.IncludeInTotal,
            OnlyForSavings = command.OnlyForSavings,
        };
}

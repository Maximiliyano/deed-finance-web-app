using Deed.Application.Debts.Commands.Create;
using Deed.Application.Debts.Commands.Update;
using Deed.Application.Debts.Responses;
using Deed.Domain.Entities;
using Deed.Domain.Enums;

namespace Deed.Application.Debts;

internal static class DebtExtensions
{
    internal static DebtResponse ToResponse(this Debt debt)
        => new(
            debt.Id,
            debt.Item,
            debt.Amount,
            debt.Currency.ToString(),
            debt.Source,
            debt.Recipient,
            debt.BorrowedAt,
            debt.DeadlineAt,
            debt.Note,
            debt.IsPaid,
            debt.CapitalId,
            debt.Capital?.Name,
            debt.OrderIndex
        );

    internal static IEnumerable<DebtResponse> ToResponses(this IEnumerable<Debt> debts)
        => debts.Select(d => d.ToResponse());

    internal static Debt ToEntity(this CreateDebtCommand cmd)
        => new()
        {
            Item = cmd.Item.Trim(),
            Amount = cmd.Amount,
            Currency = Enum.Parse<CurrencyType>(cmd.Currency),
            Source = cmd.Source.Trim(),
            Recipient = cmd.Recipient.Trim(),
            BorrowedAt = cmd.BorrowedAt,
            DeadlineAt = cmd.DeadlineAt,
            Note = cmd.Note?.Trim(),
            CapitalId = cmd.CapitalId,
        };
}

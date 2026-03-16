using Deed.Application.Debts.Commands.Create;
using Deed.Application.Debts.Commands.Update;
using Deed.Application.Debts.Responses;
using Deed.Domain.Entities;

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
            Currency = cmd.Currency,
            Source = cmd.Source.Trim(),
            Recipient = cmd.Recipient.Trim(),
            BorrowedAt = cmd.BorrowedAt,
            DeadlineAt = cmd.DeadlineAt,
            Note = cmd.Note?.Trim(),
            CapitalId = cmd.CapitalId,
        };

    internal static void ApplyUpdate(this Debt debt, UpdateDebtCommand cmd)
    {
        debt.Item = cmd.Item.Trim();
        debt.Amount = cmd.Amount;
        debt.Currency = cmd.Currency;
        debt.Source = cmd.Source.Trim();
        debt.Recipient = cmd.Recipient.Trim();
        debt.BorrowedAt = cmd.BorrowedAt;
        debt.DeadlineAt = cmd.DeadlineAt;
        debt.Note = cmd.Note?.Trim();
        debt.IsPaid = cmd.IsPaid;
    }
}

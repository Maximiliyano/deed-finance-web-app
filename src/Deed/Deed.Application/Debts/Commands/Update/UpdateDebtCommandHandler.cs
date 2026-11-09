using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.Capitals.Specifications;
using Deed.Application.Debts.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Serilog;

namespace Deed.Application.Debts.Commands.Update;

internal sealed class UpdateDebtCommandHandler(
    IDebtRepository repository,
    ICapitalRepository capitalRepository,
    IUnitOfWork unitOfWork,
    IUser user)
    : ICommandHandler<UpdateDebtCommand>
{
    public async Task<Result> Handle(UpdateDebtCommand command, CancellationToken cancellationToken)
    {
        var debt = await repository
            .GetAsync(new DebtByIdSpecification(command.Id, user.Name), cancellationToken)
            .ConfigureAwait(false);

        if (debt is null)
        {
            return Result.Failure(DomainErrors.General.NotFound("debt"));
        }

        if (HasNoChanges(command, debt))
        {
            return Result.Success();
        }

        var becomingPaid = command.IsPaid == true && !debt.IsPaid;

        debt.Item = command.Item?.Trim() ?? debt.Item;
        debt.Amount = command.Amount ?? debt.Amount;
        debt.Currency = command.Currency is null ? debt.Currency : Enum.Parse<CurrencyType>(command.Currency);
        debt.Source = command.Source?.Trim() ?? debt.Source;
        debt.Recipient = command.Recipient?.Trim() ?? debt.Recipient;
        debt.BorrowedAt = command.BorrowedAt ?? debt.BorrowedAt;
        debt.DeadlineAt = command.DeadlineAt;
        debt.Note = NormalizeNote(command.Note);
        debt.IsPaid = command.IsPaid ?? debt.IsPaid;

        if (becomingPaid && command.PayFromCapitalId.HasValue)
        {
            var capital = await capitalRepository
                .GetAsync(new CapitalByIdSpecification(command.PayFromCapitalId.Value, user.Name), cancellationToken)
                .ConfigureAwait(false);

            if (capital is null)
            {
                return Result.Failure(DomainErrors.General.NotFound("capital"));
            }

            capital.Balance -= debt.Amount;
            capitalRepository.Update(capital);
        }

        repository.Update(debt);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Log.Information("Debt {Id} updated", command.Id);

        return Result.Success();
    }

    private static bool HasNoChanges(UpdateDebtCommand command, Debt debt) =>
        (command.Item is null || command.Item.Trim().Equals(debt.Item, StringComparison.OrdinalIgnoreCase)) &&
        (command.Amount is null || command.Amount == debt.Amount) &&
        (command.Currency is null || Enum.Parse<CurrencyType>(command.Currency) == debt.Currency) &&
        (command.Source is null || command.Source.Trim().Equals(debt.Source, StringComparison.OrdinalIgnoreCase)) &&
        (command.Recipient is null || command.Recipient.Trim().Equals(debt.Recipient, StringComparison.OrdinalIgnoreCase)) &&
        (command.BorrowedAt is null || command.BorrowedAt == debt.BorrowedAt) &&
        command.DeadlineAt == debt.DeadlineAt &&
        NormalizeNote(command.Note) == debt.Note &&
        (command.IsPaid is null || command.IsPaid == debt.IsPaid);

    private static string? NormalizeNote(string? note) => string.IsNullOrWhiteSpace(note) ? null : note.Trim();
}

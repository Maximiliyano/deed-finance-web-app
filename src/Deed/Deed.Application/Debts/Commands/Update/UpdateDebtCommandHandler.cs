using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.Capitals.Specifications;
using Deed.Application.Debts.Specifications;
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

        CurrencyType? updatedCurrency = null;

        if (command.Currency is not null)
        {
            updatedCurrency = Enum.Parse<CurrencyType>(command.Currency);
        }

        var updatedItem = command.Item?.Trim();
        var updatedSource = command.Source?.Trim();
        var updatedRecipient = command.Recipient?.Trim();
        var updatedNote = command.Note?.Trim();

        if (updatedItem is null || updatedItem.Equals(debt.Item, StringComparison.OrdinalIgnoreCase) &&
            (command.Amount is null || command.Amount == debt.Amount) &&
            (updatedCurrency is null || updatedCurrency == debt.Currency) &&
            (updatedSource is null || updatedSource.Equals(debt.Source, StringComparison.OrdinalIgnoreCase)) &&
            (updatedRecipient is null || updatedRecipient.Equals(debt.Recipient, StringComparison.OrdinalIgnoreCase)) &&
            command.BorrowedAt is null || command.BorrowedAt == debt.BorrowedAt &&
            command.DeadlineAt == debt.DeadlineAt &&
            command.Note == updatedNote &&
            command.IsPaid is null || command.IsPaid == debt.IsPaid)
        {
            return Result.Success();
        }

        debt.Item = updatedItem ?? debt.Item;
        debt.Amount = command.Amount ?? debt.Amount;
        debt.Currency = updatedCurrency ?? debt.Currency;
        debt.Source = updatedSource ?? debt.Source;
        debt.Recipient = updatedRecipient ?? debt.Recipient;
        debt.BorrowedAt = command.BorrowedAt ?? debt.BorrowedAt;
        debt.DeadlineAt = command.DeadlineAt;
        debt.Note = updatedNote ?? debt.Note;
        debt.IsPaid = command.IsPaid ?? debt.IsPaid;

        var becomingPaid = command.IsPaid == true && !debt.IsPaid;

        if (becomingPaid && command.PayFromCapitalId.HasValue)
        {
            var capital = await capitalRepository
                .GetAsync(new CapitalByIdSpecification(command.PayFromCapitalId.Value), cancellationToken)
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
}

using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.Capitals.Specifications;
using Deed.Application.Debts.Specifications;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
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

        if (debt.Item == command.Item.Trim() &&
            debt.Amount == command.Amount &&
            debt.Currency == command.Currency &&
            debt.Source == command.Source.Trim() &&
            debt.Recipient == command.Recipient.Trim() &&
            debt.BorrowedAt == command.BorrowedAt &&
            debt.DeadlineAt == command.DeadlineAt &&
            debt.Note == command.Note?.Trim() &&
            debt.IsPaid == command.IsPaid)
        {
            return Result.Success();
        }

        var becomingPaid = command.IsPaid && !debt.IsPaid;

        if (becomingPaid && command.PayFromCapitalId.HasValue)
        {
            var capital = await capitalRepository
                .GetAsync(new CapitalByIdSpecification(command.PayFromCapitalId.Value), cancellationToken)
                .ConfigureAwait(false);

            if (capital is null)
            {
                return Result.Failure(DomainErrors.General.NotFound("capital"));
            }

            capital.Balance -= command.Amount;
            capitalRepository.Update(capital);
        }

        debt.ApplyUpdate(command);

        repository.Update(debt);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Log.Information("Debt {Id} updated", command.Id);

        return Result.Success();
    }
}

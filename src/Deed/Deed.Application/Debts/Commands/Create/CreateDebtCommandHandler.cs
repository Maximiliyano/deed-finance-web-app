using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.Capitals.Specifications;
using Deed.Application.Debts.Specifications;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using Serilog;

namespace Deed.Application.Debts.Commands.Create;

internal sealed class CreateDebtCommandHandler(
    IUser user,
    IDebtRepository repository,
    ICapitalRepository capitalRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateDebtCommand, int>
{
    public async Task<Result<int>> Handle(CreateDebtCommand command, CancellationToken cancellationToken)
    {
        if (!user.IsAuthenticated)
        {
            var count = await repository.CountAsync(
                new DebtsByUserSpecification(user.Name!), cancellationToken).ConfigureAwait(false);

            if (count >= AnonymousConstants.EntityLimit)
            {
                return Result.Failure<int>(DomainErrors.Anonymous.LimitReached);
            }
        }

        var debt = command.ToEntity();

        if (command.CapitalId.HasValue)
        {
            var capital = await capitalRepository
                .GetAsync(new CapitalByIdSpecification(command.CapitalId.Value), cancellationToken)
                .ConfigureAwait(false);

            if (capital is null)
            {
                return Result.Failure<int>(DomainErrors.General.NotFound("capital"));
            }

            capital.Balance += command.Amount;
            capitalRepository.Update(capital);
        }

        repository.Create(debt);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Log.Information("Debt {Id} created", debt.Id);

        return Result.Success(debt.Id);
    }
}

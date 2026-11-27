using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.Capitals.Specifications;
using Deed.Application.Debts.Specifications;
using Deed.Application.Exchanges;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using Serilog;

namespace Deed.Application.Debts.Commands.Create;

internal sealed class CreateDebtCommandHandler(
    IUser user,
    IDebtRepository repository,
    ICapitalRepository capitalRepository,
    IExchangeRateProvider exchangeRateProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateDebtCommand, int>
{
    public async Task<Result<int>> Handle(CreateDebtCommand command, CancellationToken cancellationToken)
    {
        if (!user.IsAuthenticated)
        {
            var count = await repository.CountAsync(
                new DebtsByUserSpecification(user.Name!), cancellationToken).ConfigureAwait(false);

            if (count >= AuthConstants.EntityLimit)
            {
                return Result.Failure<int>(DomainErrors.Anonymous.LimitReached);
            }
        }

        var debt = command.ToEntity();

        if (command.CapitalId.HasValue)
        {
            var capital = await capitalRepository
                .GetAsync(new CapitalByIdSpecification(command.CapitalId.Value, user.Name), cancellationToken)
                .ConfigureAwait(false);

            if (capital is null)
            {
                return Result.Failure<int>(DomainErrors.General.NotFound("capital"));
            }

            var amount = await exchangeRateProvider
                .ConvertAsync(debt.Amount, debt.Currency, capital.Currency, cancellationToken)
                .ConfigureAwait(false);

            if (!amount.IsSuccess)
            {
                return Result.Failure<int>(amount.Errors);
            }

            capital.Balance += amount.Value;
            capitalRepository.Update(capital);
        }

        repository.Create(debt);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Log.Information("Debt {Id} created", debt.Id);

        return Result.Success(debt.Id);
    }
}

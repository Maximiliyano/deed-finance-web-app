using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.Capitals.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.Capitals.Commands.Update;

internal sealed class UpdateCapitalCommandHandler(ICapitalRepository repository, IUnitOfWork unitOfWork, IUser user)
    : ICommandHandler<UpdateCapitalCommand>
{
    public async Task<Result> Handle(UpdateCapitalCommand command, CancellationToken cancellationToken)
    {
        var capital = await repository.GetAsync(new CapitalByIdSpecification(command.Id, user.Name), cancellationToken).ConfigureAwait(false);

        if (capital is null)
        {
            return Result.Failure(DomainErrors.General.NotFound(nameof(capital)));
        }

        if (HasNoChanges(command, capital))
        {
            return Result.Success();
        }

        capital.Name = command.Name?.Trim() ?? capital.Name;
        capital.Balance = command.Balance ?? capital.Balance;
        capital.Currency = command.Currency is null ? capital.Currency : Enum.Parse<CurrencyType>(command.Currency);
        capital.IncludeInTotal = command.IncludeInTotal ?? capital.IncludeInTotal;
        capital.OnlyForSavings = command.OnlyForSavings ?? capital.OnlyForSavings;

        repository.Update(capital);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }

    private static bool HasNoChanges(UpdateCapitalCommand command, Capital capital) =>
        (command.Name is null || command.Name.Trim() == capital.Name) &&
        (command.Balance is null || command.Balance == capital.Balance) &&
        (command.Currency is null || Enum.Parse<CurrencyType>(command.Currency) == capital.Currency) &&
        (command.IncludeInTotal is null || command.IncludeInTotal == capital.IncludeInTotal) &&
        (command.OnlyForSavings is null || command.OnlyForSavings == capital.OnlyForSavings);
}

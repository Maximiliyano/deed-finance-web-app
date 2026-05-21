using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.Capitals.Specifications;
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

        if ((command.Name is null || command.Name.Trim() == capital.Name) &&
            (command.Balance is null || command.Balance == capital.Balance) &&
            (command.Currency is null || command.Currency == capital.Currency) &&
            (command.IncludeInTotal is null || command.IncludeInTotal == capital.IncludeInTotal) &&
            (command.OnlyForSavings is null || command.OnlyForSavings == capital.OnlyForSavings))
        {
            return Result.Success();
        }

        capital.Name = command.Name?.Trim() ?? capital.Name;
        capital.Balance = command.Balance ?? capital.Balance;
        capital.Currency = command.Currency ?? capital.Currency;
        capital.IncludeInTotal = command.IncludeInTotal ?? capital.IncludeInTotal;
        capital.OnlyForSavings = command.OnlyForSavings ?? capital.OnlyForSavings;

        repository.Update(capital);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}

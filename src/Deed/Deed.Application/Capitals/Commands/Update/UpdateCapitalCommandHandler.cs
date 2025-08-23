using Deed.Application.Abstractions.Messaging;
using Deed.Application.Capitals.Specifications;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.Capitals.Commands.Update;

internal sealed class UpdateCapitalCommandHandler(ICapitalRepository repository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateCapitalCommand>
{
    public async Task<Result> Handle(UpdateCapitalCommand command, CancellationToken cancellationToken)
    {
        var capital = await repository.GetAsync(new CapitalByIdSpecification(command.Id));

        if (capital is null)
        {
            return Result.Failure(DomainErrors.General.NotFound(nameof(capital)));
        }

        capital.Name = command.Name?.Trim() ?? capital.Name;
        capital.Balance = command.Balance ?? capital.Balance;
        capital.Currency = command.Currency ?? capital.Currency;
        capital.IncludeInTotal = command.IncludeInTotal ?? capital.IncludeInTotal;

        repository.Update(capital);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.Capitals.Specifications;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.Capitals.Commands.Delete;

internal sealed class DeleteCapitalCommandHandler(
    ICapitalRepository repository,
    IUnitOfWork unitOfWork,
    IUser user)
    : ICommandHandler<DeleteCapitalCommand>
{
    public async Task<Result> Handle(DeleteCapitalCommand command, CancellationToken cancellationToken)
    {
        var capital = await repository.GetAsync(new CapitalByIdSpecification(command.Id, user.Name, includeExpenses: true, includeIncomes: true, includeTransfersIn: true, includeTransfersOut: true), cancellationToken).ConfigureAwait(false);

        if (capital is null)
        {
            return Result.Failure(DomainErrors.General.NotFound(nameof(capital)));
        }

        if (capital.HasReferences())
        {
            return Result.Failure(DomainErrors.Capital.ReferenceExists);
        }

        repository.Delete(capital);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}

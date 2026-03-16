using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.Capitals.Commands.PatchIncludeInTotal;

internal sealed class PatchCapitalIncludeInTotalCommandHandler(ICapitalRepository repository, IUser user)
    : ICommandHandler<PatchCapitalIncludeInTotalCommand>
{
    public async Task<Result> Handle(PatchCapitalIncludeInTotalCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(user.Name);

        var isUpdateSuccessfull = await repository.PatchIncludeInTotalAsync(request.Id, request.IncludeInTotal, user.Name, cancellationToken);

        if (!isUpdateSuccessfull)
        {
            return Result.Failure(DomainErrors.General.UpdateFailed);
        }

        return Result.Success();
    }
}

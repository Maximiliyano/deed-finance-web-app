using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.Capitals.Commands.PatchOnlyForSavings;

internal sealed class PatchCapitalSetForSavingsCommandHandler(ICapitalRepository repository, IUser user)
    : ICommandHandler<PatchCapitalSetForSavingsCommand>
{
    public async Task<Result> Handle(PatchCapitalSetForSavingsCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(user.Name);

        var isUpdateSuccessfull = await repository.PatchSavingsOnlyAsync(request.Id, request.OnlyForSavings, user.Name, cancellationToken);

        if (!isUpdateSuccessfull)
        {
            return Result.Failure(DomainErrors.General.UpdateFailed);
        }

        return Result.Success();
    }
}

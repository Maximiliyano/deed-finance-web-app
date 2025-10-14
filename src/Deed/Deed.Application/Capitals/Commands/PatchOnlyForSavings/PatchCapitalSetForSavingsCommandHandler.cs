using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deed.Application.Abstractions.Messaging;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.Capitals.Commands.PatchOnlyForSavings;

internal sealed class PatchCapitalSetForSavingsCommandHandler(ICapitalRepository repository)
    : ICommandHandler<PatchCapitalSetForSavingsCommand>
{
    public async Task<Result> Handle(PatchCapitalSetForSavingsCommand request, CancellationToken cancellationToken)
    {
        var isUpdateSuccessfull = await repository.PatchSavingsOnlyAsync(request.Id, request.OnlyForSavings, cancellationToken);

        if (!isUpdateSuccessfull)
        {
            return Result.Failure(DomainErrors.General.UpdateFailed);
        }

        return Result.Success();
    }
}

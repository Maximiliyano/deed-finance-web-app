using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deed.Application.Abstractions.Messaging;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.Capitals.Commands.PatchIncludeInTotal;

internal sealed class PatchCapitalIncludeInTotalCommandHandler(ICapitalRepository repository)
    : ICommandHandler<PatchCapitalIncludeInTotalCommand>
{
    public async Task<Result> Handle(PatchCapitalIncludeInTotalCommand request, CancellationToken cancellationToken)
    {
        var isUpdateSuccessfull = await repository.PatchIncludeInTotalAsync(request.Id, request.IncludeInTotal, cancellationToken);

        if (!isUpdateSuccessfull)
        {
            return Result.Failure(DomainErrors.General.UpdateFailed);
        }

        return Result.Success();
    }
}

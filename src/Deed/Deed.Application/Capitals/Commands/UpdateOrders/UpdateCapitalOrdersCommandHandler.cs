using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deed.Application.Abstractions.Messaging;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using MediatR;

namespace Deed.Application.Capitals.Commands.UpdateOrders;

internal sealed class UpdateCapitalOrdersCommandHandler(
    ICapitalRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateCapitalOrdersCommand>
{
    public async Task<Result> Handle(UpdateCapitalOrdersCommand request, CancellationToken cancellationToken)
    {
        await repository.UpdateOrderIndexesAsync([.. request.Capitals.Select(c => (c.Id, c.OrderIndex))], cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}

﻿using System;
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
        await repository.UpdateOrderIndexes(request.Capitals.Select(c => (c.Id, c.OrderIndex)));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

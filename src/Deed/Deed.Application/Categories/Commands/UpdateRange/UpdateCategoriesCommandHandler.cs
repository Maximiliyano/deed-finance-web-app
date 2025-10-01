using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deed.Application.Abstractions.Messaging;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.Categories.Commands.UpdateRange;

public sealed class UpdateCategoriesCommandHandler(
    ICategoryRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateCategoriesCommand>
{
    public async Task<Result> Handle(UpdateCategoriesCommand request, CancellationToken cancellationToken)
    {
        repository.UpdateRange(request.Requests.Select(x => x.ToEntity()));
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}

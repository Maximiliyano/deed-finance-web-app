using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deed.Application.Abstractions.Messaging;
using Deed.Application.Tags.Specifications;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.Tags.Commands.Delete;

internal sealed class DeleteTagCommandHandler(
    ITagRepository tagRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<DeleteTagCommand>
{
    public async Task<Result> Handle(DeleteTagCommand command, CancellationToken cancellationToken)
    {
        var tag = await tagRepository.GetAsync(new TagByIdSpecification(command.Id));

        if (tag is null)
        {
            return Result.Failure(DomainErrors.General.NotFound(nameof(tag)));
        }

        tagRepository.Delete(tag);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

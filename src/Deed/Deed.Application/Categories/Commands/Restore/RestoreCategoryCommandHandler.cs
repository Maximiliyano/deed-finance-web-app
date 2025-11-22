using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deed.Application.Abstractions.Messaging;
using Deed.Application.Categories.Response;
using Deed.Application.Categories.Specifications;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.Categories.Commands.Restore;

internal sealed class RestoreCategoryCommandHandler( // TODO write tests
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork
    ) : ICommandHandler<RestoreCategoryCommand, CategoryResponse>
{
    public async Task<Result<CategoryResponse>> Handle(
        RestoreCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetAsync(new CategoryByIdSpecification(command.Id, ignoreQueryFilter: true, tracking: true)).ConfigureAwait(false);

        if (category is null)
        {
            return Result.Failure<CategoryResponse>(DomainErrors.General.NotFound(nameof(category)));
        }

        category.IsDeleted = false;

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success(category.ToResponse());
    }
}

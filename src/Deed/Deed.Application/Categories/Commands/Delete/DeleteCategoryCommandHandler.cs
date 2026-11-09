using Deed.Application.Abstractions.Caching;
using Deed.Application.Abstractions.Messaging;
using Deed.Application.Categories.Specifications;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.Categories.Commands.Delete;

internal sealed class DeleteCategoryCommandHandler(
    ICategoryRepository repository,
    IUnitOfWork unitOfWork,
    ICacheService cache)
    : ICommandHandler<DeleteCategoryCommand>
{
    public async Task<Result> Handle(DeleteCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await repository.GetAsync(new CategoryByIdSpecification(command.Id, tracking: true), cancellationToken).ConfigureAwait(false);

        if (category is null)
        {
            return Result.Failure(DomainErrors.General.NotFound(nameof(category)));
        }

        if (await repository.HasReferencesAsync(category.Id, cancellationToken).ConfigureAwait(false))
        {
            return Result.Failure(DomainErrors.Category.ReferenceExists);
        }

        repository.Delete(category);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await cache.RemoveCategoriesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}

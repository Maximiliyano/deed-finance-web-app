using Deed.Application.Abstractions.Caching;
using Deed.Application.Abstractions.Messaging;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using Serilog;

namespace Deed.Application.Categories.Commands.Create;

internal sealed class CreateCategoryCommandHandler(
    ICategoryRepository repository,
    IUnitOfWork unitOfWork,
    ICacheService cache)
    : ICommandHandler<CreateCategoryCommand, int>
{
    public async Task<Result<int>> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = command.ToEntity();

        repository.Create(category);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await cache.RemoveCategoriesAsync(cancellationToken).ConfigureAwait(false);

        Log.Information("Category {Name} - {Id} created successfully", category.Name, category.Id);

        return category.Id;
    }
}

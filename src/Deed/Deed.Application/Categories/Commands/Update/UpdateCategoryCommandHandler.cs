using Deed.Application.Abstractions.Messaging;
using Deed.Application.Categories.Specifications;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.Categories.Commands.Update;

internal sealed class UpdateCategoryCommandHandler(
    ICategoryRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateCategoryCommand>
{
    public async Task<Result> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await repository.GetAsync(new CategoryByIdSpecification(command.Id)).ConfigureAwait(false);

        if (category is null)
        {
            return Result.Failure(DomainErrors.General.NotFound(nameof(category)));
        }

        category.Name = command.Name?.Trim() ?? category.Name;
        category.Type = command.Type ?? category.Type;
        category.PlannedPeriodAmount = command.PlannedPeriodAmount ?? category.PlannedPeriodAmount;
        category.Period = command.Period ?? category.Period;

        repository.Update(category);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}

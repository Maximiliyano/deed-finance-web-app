using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deed.Application.Abstractions.Messaging;
using Deed.Application.Categories.Specifications;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.Categories.Commands.UpdateRange;

public sealed class UpdateCategoriesCommandHandler(
    ICategoryRepository repository,
    IUnitOfWork unitOfWork
    ) : ICommandHandler<UpdateCategoriesCommand>
{
    public async Task<Result> Handle(UpdateCategoriesCommand request, CancellationToken cancellationToken)
    {
        var ids = request.Requests.Select(r => r.Id).ToList();
        var categories = await repository.GetAllAsync(ids: ids, tracking: true).ConfigureAwait(false);
        if (!categories.Any())
        {
            return Result.Success();
        }

        foreach (var updatedCategory in request.Requests)
        {
            var selectedCategory = categories.FirstOrDefault(c => c.Id.Equals(updatedCategory.Id));
            if (selectedCategory == null)
            {
                continue;
            }

            selectedCategory.Name = updatedCategory.Name ?? selectedCategory.Name;
            selectedCategory.Type = updatedCategory.Type ?? selectedCategory.Type;
            selectedCategory.Name = updatedCategory.Name ?? selectedCategory.Name;
            selectedCategory.Period = updatedCategory.PeriodType ?? selectedCategory.Period;
            selectedCategory.PlannedPeriodAmount = updatedCategory.PeriodAmount ?? selectedCategory.PlannedPeriodAmount;
        }
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}

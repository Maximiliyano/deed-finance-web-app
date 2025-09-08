using Deed.Application.Abstractions.Messaging;
using Deed.Application.Categories.Response;
using Deed.Application.Categories.Specifications;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.Categories.Queries.GetById;

internal sealed class GetByIdCategoryQueryHandler(
    ICategoryRepository repository)
    : IQueryHandler<GetByIdCategoryQuery, CategoryResponse>
{
    public async Task<Result<CategoryResponse>> Handle(GetByIdCategoryQuery query, CancellationToken cancellationToken)
    {
        var category = await repository.GetAsync(new CategoryByIdSpecification(query.Id)).ConfigureAwait(false);

        if (category is null)
        {
            return Result.Failure<CategoryResponse>(DomainErrors.General.NotFound(nameof(category)));
        }

        return Result.Success(category.ToResponse());
    }
}

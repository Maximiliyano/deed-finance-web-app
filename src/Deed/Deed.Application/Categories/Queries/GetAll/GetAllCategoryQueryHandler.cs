using Deed.Application.Abstractions.Messaging;
using Deed.Application.Abstractions.Settings;
using Deed.Application.Categories.Response;
using Deed.Application.Categories.Specifications;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Deed.Application.Categories.Queries.GetAll;

internal sealed class GetAllCategoryQueryHandler(
    ICategoryRepository repository)
    : IQueryHandler<GetAllCategoryQuery, IEnumerable<CategoryResponse>>
{
    public async Task<Result<IEnumerable<CategoryResponse>>> Handle(GetAllCategoryQuery query, CancellationToken cancellationToken)
    {
        var categories = (await repository.GetAllAsync(new CategoriesByQuerySpecification([], type: query.Type, includeDeleted: query.IncludeDeleted)).ConfigureAwait(false)).ToResponses();

        return Result.Success(categories);
    }
}

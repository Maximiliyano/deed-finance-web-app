using Deed.Application.Abstractions.Caching;
using Deed.Application.Abstractions.Messaging;
using Deed.Application.Abstractions.Settings;
using Deed.Application.Categories.Response;
using Deed.Application.Categories.Specifications;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using Microsoft.Extensions.Options;

namespace Deed.Application.Categories.Queries.GetAll;

internal sealed class GetAllCategoryQueryHandler(
    ICategoryRepository repository,
    ICacheService cache,
    IOptions<MemoryCacheSettings> settings)
    : IQueryHandler<GetAllCategoryQuery, IEnumerable<CategoryResponse>>
{
    public async Task<Result<IEnumerable<CategoryResponse>>> Handle(GetAllCategoryQuery query, CancellationToken cancellationToken)
    {
        var cacheKey = $"{CacheKeys.Categories}:{query.Type}:{query.IncludeDeleted}";

        var cached = await cache.GetAsync<List<CategoryResponse>>(cacheKey, cancellationToken).ConfigureAwait(false);
        if (cached is not null)
        {
            return Result.Success<IEnumerable<CategoryResponse>>(cached);
        }

        var categories = (await repository.GetAllAsync(new CategoriesByQuerySpecification([], type: query.Type, includeDeleted: query.IncludeDeleted), cancellationToken).ConfigureAwait(false)).ToResponses().ToList();

        await cache.SetAsync(cacheKey, categories, TimeSpan.FromHours(settings.Value.CategoriesTimespanInHours), cancellationToken).ConfigureAwait(false);

        return Result.Success<IEnumerable<CategoryResponse>>(categories);
    }
}

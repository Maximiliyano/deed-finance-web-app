using Deed.Application.Abstractions.Messaging;
using Deed.Application.Abstractions.Settings;
using Deed.Application.Categories.Response;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Deed.Application.Categories.Queries.GetAll;

internal sealed class GetAllCategoryQueryHandler(
    IOptions<MemoryCacheSettings> settings,
    IMemoryCache memoryCache,
    ICategoryRepository repository)
    : IQueryHandler<GetAllCategoryQuery, IEnumerable<CategoryResponse>>
{
    public async Task<Result<IEnumerable<CategoryResponse>>> Handle(GetAllCategoryQuery query, CancellationToken cancellationToken)
    {
        if (!memoryCache.TryGetValue(nameof(Categories), out IEnumerable<CategoryResponse> cachedCategories))
        {
            var categories = (await repository.GetAllAsync(query.Type)).ToResponses();

            memoryCache.Set<IEnumerable<CategoryResponse>>(nameof(Categories), categories, TimeSpan.FromHours(settings.Value.CategoriesTimespanInHours));

            return Result.Success(categories);
        }

        return Result.Success(cachedCategories!);
    }
}

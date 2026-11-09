namespace Deed.Application.Abstractions.Caching;

public static class CacheServiceExtensions
{
    public static Task RemoveCategoriesAsync(this ICacheService cache, CancellationToken ct = default) =>
        Task.WhenAll(CacheKeys.AllCategoryKeys().Select(key => cache.RemoveAsync(key, ct)));
}

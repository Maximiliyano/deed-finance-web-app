using System.Text.Json;
using Deed.Application.Abstractions.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Deed.Infrastructure.Caching;

internal sealed class RedisCacheService(
    IDistributedCache cache,
    ILogger<RedisCacheService> logger)
    : ICacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        try
        {
            string? json = await cache.GetStringAsync(key, ct).ConfigureAwait(false);
            return json is null ? default : JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis GET failed for key {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken ct = default)
    {
        try
        {
            string json = JsonSerializer.Serialize(value, JsonOptions);
            await cache.SetStringAsync(key, json, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            }, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis SET failed for key {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        try
        {
            await cache.RemoveAsync(key, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis REMOVE failed for key {Key}", key);
        }
    }
}

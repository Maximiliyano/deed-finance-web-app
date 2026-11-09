using Deed.Application.Abstractions.Caching;
using Deed.Application.Abstractions.Settings;
using Deed.Application.Exchanges.Responses;
using Deed.Application.Exchanges.Specifications;
using Deed.Domain.Repositories;
using Microsoft.Extensions.Options;
using Serilog;

namespace Deed.Application.Exchanges;

internal sealed class ExchangeRateProvider(
    IOptions<MemoryCacheSettings> settings,
    IExchangeRepository repository,
    ICacheService cache)
    : IExchangeRateProvider
{
    public async Task<IReadOnlyList<ExchangeResponse>> GetLatestAsync(CancellationToken cancellationToken = default)
    {
        var cached = await cache.GetAsync<List<ExchangeResponse>>(CacheKeys.Exchanges, cancellationToken).ConfigureAwait(false);

        if (cached is not null)
        {
            return cached;
        }

        var exchanges = (await repository.GetAllAsync(new ExchangesByQuerySpecification(), cancellationToken).ConfigureAwait(false)).ToResponses().ToList();

        if (exchanges.Count == 0)
        {
            Log.Error("Exchanges list are empty. Probably error happen in exchange backgroundJob.");
            return exchanges;
        }

        await cache.SetAsync(CacheKeys.Exchanges, exchanges, TimeSpan.FromHours(settings.Value.ExchangesTimespanInHours), cancellationToken).ConfigureAwait(false);

        return exchanges;
    }
}

using Deed.Application.Abstractions.Caching;
using Deed.Application.Abstractions.Messaging;
using Deed.Application.Abstractions.Settings;
using Deed.Application.Exchanges.Responses;
using Deed.Application.Exchanges.Specifications;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using Microsoft.Extensions.Options;
using Serilog;

namespace Deed.Application.Exchanges.Queries.GetLatest;

public sealed class GetLatestExchangeQueryHandler(
    IOptions<MemoryCacheSettings> settings,
    IExchangeRepository repository,
    ICacheService cache)
    : IQueryHandler<GetLatestExchangeQuery, IEnumerable<ExchangeResponse>>
{
    public async Task<Result<IEnumerable<ExchangeResponse>>> Handle(GetLatestExchangeQuery query, CancellationToken cancellationToken)
    {
        var cached = await cache.GetAsync<List<ExchangeResponse>>(CacheKeys.Exchanges, cancellationToken).ConfigureAwait(false);
        if (cached is not null)
        {
            return Result.Success<IEnumerable<ExchangeResponse>>(cached);
        }

        var actualExchanges = (await repository.GetAllAsync(new ExchangesByQuerySpecification(), cancellationToken).ConfigureAwait(false)).ToResponses().ToList();

        if (actualExchanges.Count == 0)
        {
            Log.Error("Exchanges list are empty. Probably error happen in exchange backgroundJob.");
            return Result.Success(Enumerable.Empty<ExchangeResponse>());
        }

        await cache.SetAsync(CacheKeys.Exchanges, actualExchanges, TimeSpan.FromHours(settings.Value.ExchangesTimespanInHours), cancellationToken).ConfigureAwait(false);

        return Result.Success<IEnumerable<ExchangeResponse>>(actualExchanges);
    }
}

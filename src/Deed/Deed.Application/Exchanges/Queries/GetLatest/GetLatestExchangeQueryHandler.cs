using Deed.Application.Abstractions.Messaging;
using Deed.Application.Exchanges.Responses;
using Deed.Domain.Results;

namespace Deed.Application.Exchanges.Queries.GetLatest;

public sealed class GetLatestExchangeQueryHandler(IExchangeRateProvider provider)
    : IQueryHandler<GetLatestExchangeQuery, IEnumerable<ExchangeResponse>>
{
    public async Task<Result<IEnumerable<ExchangeResponse>>> Handle(GetLatestExchangeQuery query, CancellationToken cancellationToken) =>
        Result.Success<IEnumerable<ExchangeResponse>>(await provider.GetLatestAsync(cancellationToken).ConfigureAwait(false));
}

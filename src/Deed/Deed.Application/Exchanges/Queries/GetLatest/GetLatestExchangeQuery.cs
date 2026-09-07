using Deed.Application.Abstractions.Messaging;
using Deed.Application.Exchanges.Responses;

namespace Deed.Application.Exchanges.Queries.GetLatest;

public sealed record GetLatestExchangeQuery
    : IQuery<IEnumerable<ExchangeResponse>>;

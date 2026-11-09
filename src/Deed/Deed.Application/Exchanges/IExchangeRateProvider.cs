using Deed.Application.Exchanges.Responses;

namespace Deed.Application.Exchanges;

public interface IExchangeRateProvider
{
    Task<IReadOnlyList<ExchangeResponse>> GetLatestAsync(CancellationToken cancellationToken = default);
}

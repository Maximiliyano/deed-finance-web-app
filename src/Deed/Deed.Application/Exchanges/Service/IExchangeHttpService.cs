using Deed.Domain.Entities;
using Deed.Domain.Results;

namespace Deed.Application.Exchanges.Service;

public interface IExchangeHttpService
{
    Task<Result<IEnumerable<Exchange>>> GetCurrenciesAsync();
}

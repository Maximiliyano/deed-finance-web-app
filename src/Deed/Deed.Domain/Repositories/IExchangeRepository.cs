using Deed.Domain.Entities;

namespace Deed.Domain.Repositories;

public interface IExchangeRepository
{
    void AddRange(IEnumerable<Exchange> entities);

    void UpdateRange(IEnumerable<Exchange> entities);

    Task<IEnumerable<Exchange>> GetAllAsync();
}

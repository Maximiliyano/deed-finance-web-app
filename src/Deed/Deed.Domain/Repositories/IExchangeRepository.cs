using Deed.Domain.Entities;

namespace Deed.Domain.Repositories;

public interface IExchangeRepository
{
    void CreateRange(IEnumerable<Exchange> entities);

    void UpdateRange(IEnumerable<Exchange> entities);

    Task<IEnumerable<Exchange>> GetAllAsync();
}

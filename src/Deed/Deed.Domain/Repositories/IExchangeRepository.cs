using Deed.Domain.Entities;

namespace Deed.Domain.Repositories;

public interface IExchangeRepository
{
    Task UpsertAsync(IEnumerable<Exchange> updatedExchanges, CancellationToken cancellationToken = default);

    Task<IEnumerable<Exchange>> GetAllAsync();
}

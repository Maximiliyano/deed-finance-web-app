using Deed.Application.Abstractions.Data;
using Deed.Domain.Entities;
using Deed.Domain.Repositories;

namespace Deed.Infrastructure.Persistence.Repositories;

internal sealed class IncomeRepository(
    IDeedDbContext context)
    : GeneralRepository<Income>(context), IIncomeRepository
{
    public async Task<Income?> GetAsync(ISpecification<Income> specification)
        => await base.GetAsync(specification);

    public async Task<IEnumerable<Income>> GetAllAsync()
        => await base.GetAllAsync();

    public void Create(Income income)
        => base.Create(income);

    public void Update(Income income)
        => base.Update(income);

    public void Delete(Income income)
        => base.Delete(income);
}

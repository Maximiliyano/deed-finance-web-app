using Deed.Domain.Entities;

namespace Deed.Domain.Repositories;

public interface IIncomeRepository
{
    Task<IEnumerable<Income>> GetAllAsync(ISpecification<Income> specification);

    Task<Income?> GetAsync(ISpecification<Income> specification);

    void Create(Income income);

    void Update(Income income);

    void Delete(Income income);
}

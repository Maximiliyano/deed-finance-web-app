using Deed.Domain.Entities;
using Deed.Domain.Enums;

namespace Deed.Domain.Repositories;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync(ISpecification<Category> specification, CancellationToken cancellationToken = default);

    Task<Category?> GetAsync(ISpecification<Category> specification, CancellationToken cancellationToken = default);

    void Create(Category category);

    void Update(Category category);

    void Delete(Category category);

    Task<bool> AnyAsync(ISpecification<Category> specification, CancellationToken cancellationToken = default);
}

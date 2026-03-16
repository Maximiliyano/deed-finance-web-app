using Deed.Application.Abstractions;
using Deed.Domain.Entities;

namespace Deed.Application.Categories.Specifications;

internal sealed class CategoryByNameSpecification(string name, int? excludeId = null)
    : BaseSpecification<Category>(c => c.Name == name && (excludeId == null || c.Id != excludeId));

using Deed.Application.Abstractions;
using Deed.Domain.Entities;

namespace Deed.Application.Capitals.Specifications;

internal sealed class CapitalByNameSpecification(string name, string? createdBy = null, int? excludeId = null)
    : BaseSpecification<Capital>(
        c => c.Name == name
          && (createdBy == null || c.CreatedBy == createdBy)
          && (excludeId == null || c.Id != excludeId));

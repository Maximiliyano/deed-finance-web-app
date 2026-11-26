using Deed.Application.Abstractions;
using Deed.Domain.Entities;

namespace Deed.Application.Tags.Specifications;

internal sealed class TagByIdSpecification(int id)
    : BaseSpecification<Tag>(t => t.Id.Equals(id));

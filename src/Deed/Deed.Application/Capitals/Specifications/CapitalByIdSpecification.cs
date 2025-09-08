using Deed.Application.Abstractions;
using Deed.Domain.Entities;

namespace Deed.Application.Capitals.Specifications;

internal sealed class CapitalByIdSpecification : BaseSpecification<Capital>
{
    public CapitalByIdSpecification(int id)
        : base(c => c.Id == id, true)
    {
    }
}

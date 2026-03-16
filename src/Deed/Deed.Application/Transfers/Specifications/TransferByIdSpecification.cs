using Deed.Application.Abstractions;
using Deed.Domain.Entities;

namespace Deed.Application.Transfers.Specifications;

internal sealed class TransferByIdSpecification : BaseSpecification<Transfer>
{
    public TransferByIdSpecification(int id, string? createdBy = null)
        : base(t => t.Id == id && (createdBy == null || t.CreatedBy == createdBy))
    {
    }
}

using Deed.Application.Abstractions;
using Deed.Domain.Entities;

namespace Deed.Application.Transfers.Specifications;

internal sealed class TransfersByUserSpecification : BaseSpecification<Transfer>
{
    public TransfersByUserSpecification(string createdBy)
        : base(t => t.CreatedBy == createdBy)
    {
        AddInclude(t => t.SourceCapital!);
        AddInclude(t => t.DestinationCapital!);
        ApplyOrderByDescending(t => t.CreatedAt);
    }
}

namespace Deed.Domain.Entities;

public sealed class Transfer
    : Entity, IAuditableEntity, ISoftDeletableEntity
{
    public required decimal Amount { get; set; }

    public int? SourceCapitalId { get; init; }

    public Capital? SourceCapital { get; init; }

    public int? DestinationCapitalId { get; init; }

    public Capital? DestinationCapital { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public int CreatedBy { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public int? UpdatedBy { get; init; }

    public DateTimeOffset? DeletedAt { get; init; }

    public bool? IsDeleted { get; init; }
}

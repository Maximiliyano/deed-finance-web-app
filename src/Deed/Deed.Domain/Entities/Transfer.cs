namespace Deed.Domain.Entities;

public sealed class Transfer
    : Entity, IAuditableEntity, ISoftDeletableEntity
{
    public required decimal Amount { get; set; }

    public decimal DestinationAmount { get; set; }

    public required int SourceCapitalId { get; init; }

    public Capital? SourceCapital { get; init; }

    public required int DestinationCapitalId { get; init; }

    public Capital? DestinationCapital { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public string CreatedBy { get; init; } = string.Empty;

    public DateTimeOffset? UpdatedAt { get; init; }

    public string? UpdatedBy { get; init; }

    public bool IsDeleted { get; set; }
}

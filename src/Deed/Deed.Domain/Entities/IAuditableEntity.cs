namespace Deed.Domain.Entities;

public interface IAuditableEntity
{
    DateTimeOffset CreatedAt { get; init; }

    string? CreatedBy { get; init; }

    DateTimeOffset? UpdatedAt { get; init; }

    string? UpdatedBy { get; init; }
}

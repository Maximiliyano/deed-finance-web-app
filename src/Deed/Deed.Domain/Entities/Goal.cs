using Deed.Domain.Enums;

namespace Deed.Domain.Entities;

public sealed class Goal : Entity, IAuditableEntity, ISoftDeletableEntity
{
    public Goal() { }

    public Goal(int id) : base(id) { }

    public required string Title { get; set; }

    public required decimal TargetAmount { get; set; }

    public required CurrencyType Currency { get; set; }

    public decimal CurrentAmount { get; set; }

    public DateTimeOffset? Deadline { get; set; }

    public string? Note { get; set; }

    public bool IsCompleted { get; set; }

    public int OrderIndex { get; set; }

    public DateTimeOffset CreatedAt { get; init; }

    public string CreatedBy { get; init; } = string.Empty;

    public DateTimeOffset? UpdatedAt { get; init; }

    public string? UpdatedBy { get; init; }

    public bool IsDeleted { get; set; }
}

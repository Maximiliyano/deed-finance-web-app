using Deed.Domain.Enums;

namespace Deed.Domain.Entities;

public sealed class Debt : Entity, IAuditableEntity, ISoftDeletableEntity
{
    public Debt() { }

    public Debt(int id) : base(id) { }

    public required string Item { get; set; }

    public required decimal Amount { get; set; }

    public required CurrencyType Currency { get; set; }

    public required string Source { get; set; }

    public required string Recipient { get; set; }

    public required DateTimeOffset BorrowedAt { get; set; }

    public DateTimeOffset? DeadlineAt { get; set; }

    public string? Note { get; set; }

    public bool IsPaid { get; set; }

    public int? CapitalId { get; set; }

    public Capital? Capital { get; set; }

    public int OrderIndex { get; set; }

    public DateTimeOffset CreatedAt { get; init; }

    public string CreatedBy { get; init; } = string.Empty;

    public DateTimeOffset? UpdatedAt { get; init; }

    public string? UpdatedBy { get; init; }

    public bool IsDeleted { get; set; }
}

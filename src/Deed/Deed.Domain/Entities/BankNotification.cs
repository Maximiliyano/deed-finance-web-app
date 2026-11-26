using Deed.Domain.Enums;

namespace Deed.Domain.Entities;

public sealed class BankNotification : Entity, IAuditableEntity
{
    public BankNotification() { }

    public BankNotification(int id) : base(id) { }

    public BankNotificationKind Kind { get; set; }

    public BankTransactionDirection Direction { get; set; }

    public BankTransactionType Type { get; set; }

    public string? AccountLabel { get; set; }

    public required string ExternalId { get; set; }

    public decimal Amount { get; set; }

    public string? Currency { get; set; }

    public string? Counterparty { get; set; }

    public string? Purpose { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    public bool IsRead { get; set; }

    public DateTimeOffset CreatedAt { get; init; }

    public string CreatedBy { get; init; } = string.Empty;

    public DateTimeOffset? UpdatedAt { get; init; }

    public string? UpdatedBy { get; init; }
}

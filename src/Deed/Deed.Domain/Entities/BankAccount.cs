using Deed.Domain.Enums;

namespace Deed.Domain.Entities;

public sealed class BankAccount : Entity, IAuditableEntity
{
    public BankAccount() { }

    public BankAccount(int id) : base(id) { }

    public BankProvider Provider { get; set; }

    public required string Iban { get; set; }

    public string Currency { get; set; } = string.Empty;

    public string? Name { get; set; }

    public decimal? Balance { get; set; }

    public int SourceConnectionId { get; set; }

    public DateTimeOffset LastRefreshedAt { get; set; }

    public DateTimeOffset CreatedAt { get; init; }

    public string CreatedBy { get; init; } = string.Empty;

    public DateTimeOffset? UpdatedAt { get; init; }

    public string? UpdatedBy { get; init; }
}

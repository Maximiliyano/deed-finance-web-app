using Deed.Domain.Enums;

namespace Deed.Domain.Entities;

public sealed class BankConnection : Entity, IAuditableEntity, ISoftDeletableEntity
{
    public BankConnection() { }

    public BankConnection(int id) : base(id) { }

    public BankProvider Provider { get; set; }

    public required string EncryptedToken { get; set; }

    public required string AccountIban { get; set; }

    public string? ProviderAccountId { get; set; }

    public string Label { get; set; } = string.Empty;

    public string? Currency { get; set; }

    public int? CapitalId { get; set; }

    public bool AutoImport { get; set; }

    public bool IsExcluded { get; set; }

    public int? ExpenseCategoryId { get; set; }

    public int? IncomeCategoryId { get; set; }

    public BankConnectionStatus Status { get; set; }

    public DateTimeOffset LastSyncAt { get; set; }

    public DateTimeOffset? BackfillFrom { get; set; }

    public DateTimeOffset? BackfillCursor { get; set; }

    public int BackfillFailures { get; set; }

    public DateTimeOffset CreatedAt { get; init; }

    public string CreatedBy { get; init; } = string.Empty;

    public DateTimeOffset? UpdatedAt { get; init; }

    public string? UpdatedBy { get; init; }

    public bool IsDeleted { get; set; }
}

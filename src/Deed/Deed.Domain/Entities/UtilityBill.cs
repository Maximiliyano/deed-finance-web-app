using Deed.Domain.Enums;

namespace Deed.Domain.Entities;

public sealed class UtilityBill : Entity, IAuditableEntity, ISoftDeletableEntity
{
    public UtilityBill() { }

    public UtilityBill(int id) : base(id) { }

    public required string Name { get; set; }

    public required decimal EstimatedAmount { get; set; }

    public required CurrencyType Currency { get; set; }

    public required int DueDayOfMonth { get; set; }

    public required int CapitalId { get; set; }

    public Capital? Capital { get; init; }

    public required int CategoryId { get; set; }

    public Category Category { get; init; } = null!;

    public bool IsActive { get; set; } = true;

    public List<Expense> Payments { get; init; } = [];

    public DateTimeOffset CreatedAt { get; init; }

    public string CreatedBy { get; init; } = string.Empty;

    public DateTimeOffset? UpdatedAt { get; init; }

    public string? UpdatedBy { get; init; }

    public bool IsDeleted { get; set; }
}

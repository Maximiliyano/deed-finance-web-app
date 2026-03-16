using Deed.Domain.Enums;

namespace Deed.Domain.Entities;

public sealed class BudgetEstimation : Entity, IAuditableEntity, ISoftDeletableEntity
{
    public BudgetEstimation() { }

    public BudgetEstimation(int id) : base(id) { }

    public required string Description { get; set; }

    public required decimal BudgetAmount { get; set; }

    public required CurrencyType BudgetCurrency { get; set; }

    public int? CapitalId { get; set; }

    public Capital? Capital { get; init; }

    public int OrderIndex { get; set; }

    public DateTimeOffset CreatedAt { get; init; }

    public string CreatedBy { get; init; } = string.Empty;

    public DateTimeOffset? UpdatedAt { get; init; }

    public string? UpdatedBy { get; init; }

    public bool IsDeleted { get; set; }
}

using Deed.Domain.Enums;

namespace Deed.Domain.Entities;

public sealed class Category : Entity, IAuditableEntity, ISoftDeletableEntity
{
    public Category(int id)
         : base(id)
    {
    }

    public Category()
    {
    }

    public required string Name { get; set; }

    public required CategoryType Type { get; set; }

    public decimal PlannedPeriodAmount { get; set; }

    public PerPeriodType Period { get; set; }

    public ICollection<Expense> Expenses { get; init; } = [];

    public ICollection<Income> Incomes { get; init; } = [];

    public DateTimeOffset CreatedAt { get; init; }

    public string? CreatedBy { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public string? UpdatedBy { get; init; }

    public bool? IsDeleted { get; set; }

    public bool HasReferences() =>
        Expenses.Any()
        || Incomes.Any();
}

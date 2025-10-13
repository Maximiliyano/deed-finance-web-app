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

    public float PlannedPeriodAmount { get; set; }

    public PerPeriodType Period { get; set; }

    public ICollection<Expense> Expenses { get; init; } = [];

    public ICollection<Income> Incomes { get; init; } = [];

    public DateTimeOffset CreatedAt { get; init; }

    public int CreatedBy { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public int? UpdatedBy { get; init; }

    public DateTimeOffset? DeletedAt { get; init; }

    public bool? IsDeleted { get; init; }
}

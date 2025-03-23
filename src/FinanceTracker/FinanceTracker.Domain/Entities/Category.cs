using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Domain.Entities;

public sealed class Category
    : Entity, IAuditableEntity, ISoftDeletableEntity
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

    public float TotalExpenses => Expenses?.Sum(x => x.Amount) ?? 0;

    public float TotalIncomes => Incomes?.Sum(x => x.Amount) ?? 0;

    public IEnumerable<Expense>? Expenses { get; init; }

    public IEnumerable<Income>? Incomes { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public int CreatedBy { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public int? UpdatedBy { get; init; }

    public DateTimeOffset? DeletedAt { get; init; }

    public bool? IsDeleted { get; init; }
}

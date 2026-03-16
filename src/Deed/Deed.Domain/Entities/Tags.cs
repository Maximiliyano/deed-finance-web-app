namespace Deed.Domain.Entities;

public sealed class Tag : Entity
{
    public required string Name { get; init; }

    public ICollection<ExpenseTag> ExpenseTags { get; init; } = [];

    public ICollection<IncomeTag> IncomeTags { get; init; } = [];
}

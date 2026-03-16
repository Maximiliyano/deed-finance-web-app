namespace Deed.Domain.Entities;

public sealed class IncomeTag : Entity, ISoftDeletableEntity
{
    public Income Income { get; init; } = null!;

    public int TagId { get; set; }

    public Tag Tag { get; init; } = null!;

    public bool IsDeleted { get; set; }
}

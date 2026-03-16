namespace Deed.Domain.Entities;

public sealed class Income : Entity, IAuditableEntity, ISoftDeletableEntity
{
    public Income()
    {
    }

    public Income(int id)
        : base(id)
    {
    }

    public required decimal Amount { get; set; }

    public required DateTimeOffset PaymentDate { get; set; }

    public required int CategoryId { get; set; }

    public Category? Category { get; set; }

    public required int CapitalId { get; init; }

    public Capital? Capital { get; init; }

    public string? Purpose { get; set; }

    public List<IncomeTag> Tags { get; init; } = [];

    public DateTimeOffset CreatedAt { get; init; }

    public string CreatedBy { get; init; } = string.Empty;

    public DateTimeOffset? UpdatedAt { get; init; }

    public string? UpdatedBy { get; init; }

    public bool IsDeleted { get; set; }
}

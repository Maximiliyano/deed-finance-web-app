namespace Deed.Domain.Entities;

public sealed class Expense
    : Entity, IAuditableEntity, ISoftDeletableEntity
{
    public Expense(int id)
        : base(id)
    {
    }

    public Expense()
    {
    }

    public required decimal Amount { get; set; }

    public required DateTimeOffset PaymentDate { get; set; }

    public Category Category { get; init; }

    public required int CategoryId { get; set; }

    public Capital? Capital { get; init; }

    public required int CapitalId { get; set; }

    public string? Purpose { get; set; }

    public DateTimeOffset CreatedAt { get; init; }

    public int CreatedBy { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public int? UpdatedBy { get; init; }

    public bool? IsDeleted { get; set; }
}

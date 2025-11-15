using System.ComponentModel.DataAnnotations;
using Deed.Domain.Enums;

namespace Deed.Domain.Entities;

public sealed class Capital
    : Entity, IAuditableEntity, ISoftDeletableEntity
{
    public Capital()
    {
    }

    public Capital(int id)
        : base(id)
    {
    }

    public required string Name { get; set; }

    public required decimal Balance { get; set; }

    public required CurrencyType Currency { get; set; }

    public bool IncludeInTotal { get; set; }

    public bool OnlyForSavings { get; set; }

    public int OrderIndex { get; set; }

    public DateTimeOffset CreatedAt { get; init; }

    public int CreatedBy { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public int? UpdatedBy { get; init; }

    public DateTimeOffset? DeletedAt { get; init; }

    public bool? IsDeleted { get; init; }

    public decimal TotalIncome => Incomes.Sum(i => i.Amount);

    public decimal TotalExpense => Expenses.Sum(e => e.Amount);

    public decimal TotalTransferIn => TransfersIn.Sum(t => t.Amount);

    public decimal TotalTransferOut => TransfersOut.Sum(t => t.Amount);

    public ICollection<Income> Incomes { get; } = [];

    public ICollection<Expense> Expenses { get; } = [];

    public ICollection<Transfer> TransfersIn { get; } = [];

    public ICollection<Transfer> TransfersOut { get; } = [];

    [Timestamp]
    public byte[]? RowVersion { get; set; }
}

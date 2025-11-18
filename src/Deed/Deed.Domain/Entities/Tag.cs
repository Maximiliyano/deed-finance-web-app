using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deed.Domain.Entities;

public sealed class Tag : Entity, IAuditableEntity
{
    public Tag(int id)
        : base(id)
    {
    }

    public Tag()
    {
    }

    public required string Name { get; set; }


    public DateTimeOffset CreatedAt { get; init; }
    public int CreatedBy { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    public int? UpdatedBy { get; init; }

    public required int ExpenseId { get; init; }
    public Expense? Expense { get; init; }
}

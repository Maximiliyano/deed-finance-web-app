using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Deed.Domain.Entities;

public sealed class ExpenseTag : Entity, ISoftDeletableEntity
{
    public Expense Expense { get; init; } = null!;
    
    public int TagId { get; set; }
    public Tag Tag { get; init; } = null!;

    public bool? IsDeleted { get; set; }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deed.Domain.Entities;

public sealed class Tag : Entity
{
    public string Name { get; init; }

    public ICollection<ExpenseTag> ExpenseTags { get; init; } = [];
}

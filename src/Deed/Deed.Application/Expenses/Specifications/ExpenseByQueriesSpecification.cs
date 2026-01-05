using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deed.Application.Abstractions;
using Deed.Domain.Entities;
using Deed.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Deed.Application.Expenses.Specifications;

internal sealed class ExpenseByQueriesSpecification : BaseSpecification<Expense>
{
    public ExpenseByQueriesSpecification(int? capitalId)
        : base(e => !capitalId.HasValue || e.CapitalId == capitalId.Value)
    {
        AddInclude(t => t.Capital);
        AddInclude(t => t.Category);

        Includes.Add(e => e.Include(t => t.Tags).ThenInclude(ta => ta.Tag));
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deed.Application.Abstractions;
using Deed.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Deed.Application.Tags.Specifications;

internal sealed class ExpenseTagsByQuerySpefication
    : BaseSpecification<ExpenseTag>
{
    public ExpenseTagsByQuerySpefication(string? term)
        : base(et => string.IsNullOrEmpty(term) || EF.Functions.Like(et.Tag.Name, $"%{term}%"))
    {
        AddInclude(et => et.Tag);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deed.Application.Abstractions;
using Deed.Domain.Entities;

namespace Deed.Application.Incomes.Specifications;

internal sealed class IncomesByQuerySpecification 
    : BaseSpecification<Income>
{
    public IncomesByQuerySpecification()
        : base()
    {
    }
}

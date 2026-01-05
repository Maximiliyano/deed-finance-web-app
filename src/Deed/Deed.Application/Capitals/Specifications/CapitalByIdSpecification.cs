using Deed.Application.Abstractions;
using Deed.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Deed.Application.Capitals.Specifications;

internal sealed class CapitalByIdSpecification : BaseSpecification<Capital>
{
    public CapitalByIdSpecification(
        int id,
        bool includeExpenses = false,
        bool includeIncomes = false,
        bool includeTransfersIn = false,
        bool includeTransfersOut = false
    ) : base(c => c.Id == id)
    {
        if (includeIncomes)
        {
            AddInclude(c => c.Incomes);
        }
        if (includeExpenses)
        {
            AddInclude(c => c.Expenses);
        }
        if (includeTransfersIn)
        {
            AddInclude(c => c.TransfersIn);
        }
        if (includeTransfersOut)
        {
            AddInclude(c => c.TransfersOut);
        }
    }
}

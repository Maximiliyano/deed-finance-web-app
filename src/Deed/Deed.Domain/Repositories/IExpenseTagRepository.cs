using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deed.Domain.Entities;

namespace Deed.Domain.Repositories;

public interface IExpenseTagRepository
{
    void Create(ExpenseTag tag);

    Task<IEnumerable<ExpenseTag>> GetAllAsync(ISpecification<ExpenseTag> specification);
}

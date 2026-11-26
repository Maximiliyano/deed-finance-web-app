using Deed.Application.Abstractions;
using Deed.Domain.Entities;

namespace Deed.Application.BankConnections.Specifications;

internal sealed class BankAccountByIdSpecification : BaseSpecification<BankAccount>
{
    public BankAccountByIdSpecification(int id, string createdBy)
        : base(a => a.Id == id && a.CreatedBy == createdBy)
    {
    }
}

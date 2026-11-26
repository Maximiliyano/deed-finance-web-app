using Deed.Application.Abstractions;
using Deed.Domain.Entities;


namespace Deed.Application.Exchanges.Specifications;

internal sealed class ExchangesByQuerySpecification
    : BaseSpecification<Exchange>
{
    public ExchangesByQuerySpecification()
        : base()
    {
        ApplyOrderBy(e => e.CreatedAt);

        Tracking = true;
    }
}

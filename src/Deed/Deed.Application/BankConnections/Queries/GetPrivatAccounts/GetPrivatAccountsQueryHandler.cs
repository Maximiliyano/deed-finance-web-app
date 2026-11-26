using Deed.Application.Abstractions.Messaging;
using Deed.Application.BankConnections.Service;
using Deed.Domain.Results;

namespace Deed.Application.BankConnections.Queries.GetPrivatAccounts;

internal sealed class GetPrivatAccountsQueryHandler(IPrivatAutoclientService bank)
    : IQueryHandler<GetPrivatAccountsQuery, IReadOnlyList<PrivatAccount>>
{
    public Task<Result<IReadOnlyList<PrivatAccount>>> Handle(GetPrivatAccountsQuery query, CancellationToken cancellationToken)
        => bank.GetAccountsAsync(query.Token.Trim(), cancellationToken);
}

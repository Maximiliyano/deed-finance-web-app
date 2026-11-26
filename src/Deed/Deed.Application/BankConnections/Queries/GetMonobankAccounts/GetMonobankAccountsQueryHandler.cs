using Deed.Application.Abstractions.Messaging;
using Deed.Application.BankConnections.Service;
using Deed.Domain.Results;

namespace Deed.Application.BankConnections.Queries.GetMonobankAccounts;

internal sealed class GetMonobankAccountsQueryHandler(IMonobankService bank)
    : IQueryHandler<GetMonobankAccountsQuery, IReadOnlyList<MonobankAccount>>
{
    public Task<Result<IReadOnlyList<MonobankAccount>>> Handle(GetMonobankAccountsQuery query, CancellationToken cancellationToken)
        => bank.GetAccountsAsync(query.Token.Trim(), cancellationToken);
}

using Deed.Application.Abstractions.Messaging;
using Deed.Application.BankConnections.Service;

namespace Deed.Application.BankConnections.Queries.GetPrivatAccounts;

public sealed record GetPrivatAccountsQuery(string Token) : IQuery<IReadOnlyList<PrivatAccount>>;

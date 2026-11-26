using Deed.Application.Abstractions.Messaging;
using Deed.Application.BankConnections.Service;

namespace Deed.Application.BankConnections.Queries.GetMonobankAccounts;

public sealed record GetMonobankAccountsQuery(string Token) : IQuery<IReadOnlyList<MonobankAccount>>;

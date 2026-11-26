using Deed.Application.Abstractions.Messaging;
using Deed.Application.BankConnections.Responses;

namespace Deed.Application.BankConnections.Queries.GetBankAccounts;

public sealed record GetBankAccountsQuery : IQuery<IReadOnlyList<LinkedBankAccountResponse>>;

using Deed.Application.Abstractions.Messaging;

namespace Deed.Application.BankConnections.Commands.RefreshBankAccounts;

public sealed record RefreshBankAccountsCommand : ICommand<int>;

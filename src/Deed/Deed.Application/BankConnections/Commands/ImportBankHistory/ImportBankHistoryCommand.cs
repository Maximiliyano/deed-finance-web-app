using Deed.Application.Abstractions.Messaging;

namespace Deed.Application.BankConnections.Commands.ImportBankHistory;

public sealed record ImportBankHistoryCommand : ICommand<int>;

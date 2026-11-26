using Deed.Application.Abstractions.Messaging;
using Deed.Application.BankConnections.Responses;

namespace Deed.Application.BankConnections.Commands.ImportBankHistory;

public sealed record ImportBankHistoryCommand(DateTimeOffset From, DateTimeOffset? To = null)
    : ICommand<BankHistoryImportResponse>;

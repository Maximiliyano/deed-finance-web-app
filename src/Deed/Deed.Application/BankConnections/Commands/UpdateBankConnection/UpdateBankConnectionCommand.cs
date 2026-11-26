using Deed.Application.Abstractions.Messaging;

namespace Deed.Application.BankConnections.Commands.UpdateBankConnection;

public sealed record UpdateBankConnectionCommand(
    int Id,
    string? Label,
    int? CapitalId,
    bool AutoImport,
    int? ExpenseCategoryId,
    int? IncomeCategoryId) : ICommand;

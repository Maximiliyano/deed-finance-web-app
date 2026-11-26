namespace Deed.Application.BankConnections.Requests;

public sealed record UpdateBankConnectionRequest(
    string? Label,
    int? CapitalId,
    bool AutoImport,
    int? ExpenseCategoryId,
    int? IncomeCategoryId);

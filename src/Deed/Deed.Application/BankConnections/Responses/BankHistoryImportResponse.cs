namespace Deed.Application.BankConnections.Responses;

public sealed record BankHistoryImportResponse(int Imported, bool InProgress);

namespace Deed.Application.BankConnections.Requests;

public sealed record ImportBankHistoryRequest(DateTimeOffset From, DateTimeOffset? To);

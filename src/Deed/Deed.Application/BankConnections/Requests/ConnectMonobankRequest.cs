namespace Deed.Application.BankConnections.Requests;

public sealed record ConnectMonobankRequest(string Token, string AccountId, string? Iban, string? Label);

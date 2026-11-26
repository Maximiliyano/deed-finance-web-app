namespace Deed.Application.BankConnections.Requests;

public sealed record ConnectPrivatRequest(string Token, string Iban, string? Label, string? Currency = null);

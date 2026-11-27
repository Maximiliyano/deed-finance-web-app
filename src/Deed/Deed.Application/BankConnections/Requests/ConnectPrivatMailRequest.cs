namespace Deed.Application.BankConnections.Requests;

public sealed record ConnectPrivatMailRequest(string Iban, string? Label, string? Currency = null);

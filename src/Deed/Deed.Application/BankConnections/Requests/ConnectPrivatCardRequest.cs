namespace Deed.Application.BankConnections.Requests;

public sealed record ConnectPrivatCardRequest(string MerchantId, string Password, string CardNumber);

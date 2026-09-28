namespace Deed.Application.Bank.Responses;

public sealed record BankManagedClients(
    string ClientId,
    string Tin,
    string Name,
    IEnumerable<BankAccountResponse> Accounts
);

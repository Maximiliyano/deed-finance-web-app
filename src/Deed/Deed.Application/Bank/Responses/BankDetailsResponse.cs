namespace Deed.Application.Bank.Responses;

public sealed record BankDetailsResponse(
    string ClientId,
    string Name,
    Uri WebHookUrl,
    string Permissions,
    IEnumerable<BankAccountResponse> Accounts,
    IEnumerable<BankJar> Jars,
    IEnumerable<BankManagedClients> ManagedClients
);

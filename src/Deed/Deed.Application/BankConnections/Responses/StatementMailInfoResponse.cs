namespace Deed.Application.BankConnections.Responses;

public sealed record StatementMailInfoResponse(bool Enabled, string? AddressTemplate);

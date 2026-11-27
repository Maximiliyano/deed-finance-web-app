namespace Deed.Application.BankConnections.Statements;

public sealed record StatementRow(
    DateTimeOffset OccurredAt,
    decimal Amount,
    string? Currency,
    string? Description,
    string? Counterparty,
    decimal? Balance);

namespace Deed.Application.BankConnections.Statements;

public sealed record StatementMailImportResult(StatementMailOutcome Outcome, int Created);

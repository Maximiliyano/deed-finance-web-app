namespace Deed.Application.BankConnections.Statements;

public sealed record ParsedStatement(IReadOnlyList<StatementRow> Rows, IReadOnlyList<string> Ibans, string HeaderMap);

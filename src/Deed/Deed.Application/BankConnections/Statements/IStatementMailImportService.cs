namespace Deed.Application.BankConnections.Statements;

public interface IStatementMailImportService
{
    Task<StatementMailImportResult> ImportAsync(StatementMail mail, CancellationToken cancellationToken);
}

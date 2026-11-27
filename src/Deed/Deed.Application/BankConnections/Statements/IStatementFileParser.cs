namespace Deed.Application.BankConnections.Statements;

public interface IStatementFileParser
{
    ParsedStatement? Parse(StatementAttachment attachment);
}

namespace Deed.Application.BankConnections.Statements;

public sealed record StatementAttachment(string FileName, string ContentType, byte[] Content);

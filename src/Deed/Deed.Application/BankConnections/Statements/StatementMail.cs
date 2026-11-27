namespace Deed.Application.BankConnections.Statements;

public sealed record StatementMail(
    string? From,
    string? Subject,
    string? AuthenticationResults,
    IReadOnlyList<string> Recipients,
    IReadOnlyList<StatementAttachment> Attachments,
    string? Body);

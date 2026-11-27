using Deed.Application.BankConnections.Statements;
using MimeKit;

namespace Deed.Infrastructure.BackgroundJobs.StatementMailSync;

internal static class StatementMailMapper
{
    private static readonly string[] StatementExtensions = [".csv", ".tsv", ".txt", ".xlsx", ".xls", ".htm", ".html"];
    private static readonly string[] RecipientHeaders = ["Delivered-To", "X-Original-To", "Envelope-To", "X-Envelope-To"];

    internal static StatementMail Map(MimeMessage message, int maxAttachmentBytes)
    {
        var recipients = message.To.Mailboxes
            .Concat(message.Cc.Mailboxes)
            .Select(mailbox => mailbox.Address)
            .Concat(message.Headers
                .Where(header => RecipientHeaders.Contains(header.Field, StringComparer.OrdinalIgnoreCase))
                .Select(header => header.Value))
            .ToList();

        var html = message.HtmlBody;
        var body = html is not null && html.Contains("<table", StringComparison.OrdinalIgnoreCase) ? html : message.TextBody;

        return new StatementMail(
            message.From.Mailboxes.FirstOrDefault()?.Address,
            message.Subject,
            message.Headers[HeaderId.AuthenticationResults],
            recipients,
            ReadAttachments(message, maxAttachmentBytes),
            body);
    }

    private static List<StatementAttachment> ReadAttachments(MimeMessage message, int maxAttachmentBytes)
    {
        var attachments = new List<StatementAttachment>();

        foreach (var part in message.BodyParts.OfType<MimePart>().Where(IsStatementPart))
        {
            using var stream = new MemoryStream();

            part.Content?.DecodeTo(stream);

            if (stream.Length is > 0 && stream.Length <= maxAttachmentBytes)
            {
                attachments.Add(new StatementAttachment(part.FileName ?? "statement", part.ContentType.MimeType, stream.ToArray()));
            }
        }

        return attachments;
    }

    private static bool IsStatementPart(MimePart part)
        => part.Content is not null
            && (part.IsAttachment || HasStatementExtension(part.FileName))
            && !part.ContentType.IsMimeType("message", "rfc822");

    private static bool HasStatementExtension(string? fileName)
        => fileName is not null && StatementExtensions.Contains(Path.GetExtension(fileName), StringComparer.OrdinalIgnoreCase);
}

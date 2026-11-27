using System.Text;
using Deed.Application.Abstractions.Settings;
using Deed.Application.BankConnections.Specifications;
using Deed.Application.BankConnections.Statements;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using Microsoft.Extensions.Options;
using Serilog;

namespace Deed.Application.BankConnections.Service;

internal sealed class StatementMailImportService(
    IOptions<StatementMailSettings> options,
    IStatementFileParser parser,
    IBankConnectionRepository connectionRepository,
    IBankTransactionSyncService syncService)
    : IStatementMailImportService
{
    private const string DefaultCurrency = "UAH";

    public async Task<StatementMailImportResult> ImportAsync(StatementMail mail, CancellationToken cancellationToken)
    {
        var settings = options.Value;

        if (!StatementMailPolicy.IsTrusted(mail.From, mail.AuthenticationResults, settings))
        {
            Log.Warning("Statement mail from {From} was rejected: sender or authentication results are not trusted", mail.From);

            return new StatementMailImportResult(StatementMailOutcome.Rejected, 0);
        }

        var keys = StatementMailAddress.ExtractKeys(mail.Recipients, settings.Address);

        if (keys.Count == 0)
        {
            Log.Information("Statement mail \"{Subject}\" was ignored: no connection key in its recipients", mail.Subject);

            return new StatementMailImportResult(StatementMailOutcome.Unrouted, 0);
        }

        var connections = (await connectionRepository
            .GetAllAsync(new MailBankConnectionsByKeysSpecification(keys), cancellationToken)
            .ConfigureAwait(false)).ToList();

        if (connections.Count == 0)
        {
            Log.Information("Statement mail \"{Subject}\" was ignored: no active connection matches its key", mail.Subject);

            return new StatementMailImportResult(StatementMailOutcome.Unrouted, 0);
        }

        var statements = Parse(mail);

        if (statements.Count == 0)
        {
            return new StatementMailImportResult(StatementMailOutcome.Unreadable, 0);
        }

        var created = 0;

        foreach (var connection in connections)
        {
            foreach (var statement in statements)
            {
                if (statement.Ibans.Count > 0 && !statement.Ibans.Contains(connection.AccountIban, StringComparer.OrdinalIgnoreCase))
                {
                    Log.Warning("Statement for {Ibans} does not match the connection {Label} of {User} and was skipped", string.Join(", ", statement.Ibans), connection.Label, connection.CreatedBy);

                    continue;
                }

                created += await syncService
                    .ImportStatementAsync(connection, ToTransactions(connection, statement), cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        return new StatementMailImportResult(StatementMailOutcome.Imported, created);
    }

    internal static List<BankTransaction> ToTransactions(BankConnection connection, ParsedStatement statement)
    {
        var seen = new Dictionary<string, int>(StringComparer.Ordinal);
        var transactions = new List<BankTransaction>(statement.Rows.Count);

        foreach (var row in statement.Rows)
        {
            var currency = string.IsNullOrWhiteSpace(connection.Currency) ? row.Currency ?? DefaultCurrency : connection.Currency;

            var id = StatementExternalId.Compute(
                connection.AccountIban,
                row.OccurredAt,
                row.Amount,
                currency,
                $"{row.Counterparty} {row.Description}",
                row.Balance);

            seen.TryGetValue(id, out var ordinal);
            seen[id] = ordinal + 1;

            transactions.Add(new BankTransaction(
                StatementExternalId.WithOrdinal(id, ordinal),
                row.Amount > 0 ? BankTransactionDirection.Incoming : BankTransactionDirection.Outgoing,
                Math.Abs(row.Amount),
                currency,
                row.Counterparty ?? row.Description,
                row.Counterparty is null ? null : row.Description,
                row.OccurredAt));
        }

        return transactions;
    }

    private List<ParsedStatement> Parse(StatementMail mail)
    {
        IReadOnlyList<StatementAttachment> attachments = mail.Attachments;

        if (attachments.Count == 0 && !string.IsNullOrWhiteSpace(mail.Body))
        {
            attachments = [new StatementAttachment("body", "text/plain", Encoding.UTF8.GetBytes(mail.Body))];
        }

        var statements = new List<ParsedStatement>();

        foreach (var attachment in attachments)
        {
            if (parser.Parse(attachment) is { } statement)
            {
                statements.Add(statement);
            }
        }

        return statements;
    }
}

using System.Text.RegularExpressions;
using Serilog;

namespace Deed.Application.BankConnections.Statements;

internal sealed partial class StatementFileParser : IStatementFileParser
{
    private const int PreviewRows = 3;

    private static readonly string[] OutgoingKinds = ["d", "debit", "дебет", "витрата", "списання", "outgoing", "расход"];
    private static readonly string[] IncomingKinds = ["c", "credit", "кредит", "надходження", "зарахування", "incoming", "приход"];

    public ParsedStatement? Parse(StatementAttachment attachment)
    {
        IReadOnlyList<string[]>? table;

        try
        {
            table = StatementTableReader.Read(attachment);
        }
        catch (Exception ex) when (ex is InvalidDataException or System.Xml.XmlException or InvalidOperationException)
        {
            Log.Warning(ex, "Statement file {File} is not readable", attachment.FileName);

            return null;
        }

        if (table is null || table.Count == 0)
        {
            Log.Information("Statement file {File} was skipped: no table found (legacy .xls files are not supported)", attachment.FileName);

            return null;
        }

        var map = StatementColumns.Detect(table);

        if (map is null)
        {
            Log.Warning(
                "Statement file {File} has no header with a date and an amount column; first rows: {Preview}",
                attachment.FileName,
                string.Join(" // ", table.Take(PreviewRows).Select(row => string.Join(" | ", row))));

            return null;
        }

        var rows = new List<StatementRow>();
        var skipped = 0;

        foreach (var cells in table.Skip(map.HeaderRow + 1))
        {
            if (ParseRow(map, cells) is { } row)
            {
                rows.Add(row);
            }
            else
            {
                skipped++;
            }
        }

        var ibans = FindIbans(table.Take(map.HeaderRow).SelectMany(row => row)
            .Concat(table.Skip(map.HeaderRow + 1).Select(row => map.Cell(row, StatementField.Account) ?? string.Empty)));

        var description = map.Describe(table);

        Log.Information(
            "Statement file {File}: columns [{Columns}], {Rows} row(s), {Skipped} skipped, {Ibans} account(s) named",
            attachment.FileName,
            description,
            rows.Count,
            skipped,
            ibans.Count);

        return new ParsedStatement(rows, ibans, description);
    }

    internal static IReadOnlyList<string> FindIbans(IEnumerable<string> texts)
        => [.. texts
            .SelectMany(text => IbanPattern().Matches(text).Select(match => match.Value.ToUpperInvariant()))
            .Distinct(StringComparer.Ordinal)];

    private static StatementRow? ParseRow(StatementColumnMap map, string[] cells)
    {
        var occurredAt = StatementValues.ParseDateTime(map.Cell(cells, StatementField.Date), map.Cell(cells, StatementField.Time));

        if (occurredAt is null)
        {
            return null;
        }

        var currency = ParseCurrency(map.Cell(cells, StatementField.Currency));
        decimal amount;

        if (map.Has(StatementField.Amount))
        {
            if (StatementValues.ParseAmount(map.Cell(cells, StatementField.Amount)) is not { } parsed)
            {
                return null;
            }

            amount = ApplyKind(parsed.Value, map.Cell(cells, StatementField.Kind));
            currency ??= parsed.Currency;
        }
        else
        {
            var debit = StatementValues.ParseDecimal(map.Cell(cells, StatementField.Debit));
            var credit = StatementValues.ParseDecimal(map.Cell(cells, StatementField.Credit));

            if (debit is null && credit is null)
            {
                return null;
            }

            amount = Math.Abs(credit ?? 0m) - Math.Abs(debit ?? 0m);
        }

        if (amount == 0m)
        {
            return null;
        }

        return new StatementRow(
            occurredAt.Value,
            amount,
            currency,
            Blank(map.Cell(cells, StatementField.Description)),
            Blank(map.Cell(cells, StatementField.Counterparty)),
            StatementValues.ParseDecimal(map.Cell(cells, StatementField.Balance)));
    }

    private static decimal ApplyKind(decimal amount, string? kind)
    {
        var normalized = StatementValues.NormalizeHeader(kind);

        if (OutgoingKinds.Contains(normalized, StringComparer.OrdinalIgnoreCase))
        {
            return -Math.Abs(amount);
        }

        return IncomingKinds.Contains(normalized, StringComparer.OrdinalIgnoreCase) ? Math.Abs(amount) : amount;
    }

    private static string? ParseCurrency(string? text)
    {
        var value = StatementValues.Collapse(text).ToUpperInvariant();

        if (value.Length == 3 && value.All(char.IsAsciiLetter))
        {
            return value;
        }

        return value switch
        {
            "ГРН" or "₴" => "UAH",
            "$" => "USD",
            "€" => "EUR",
            "ZŁ" => "PLN",
            "KČ" => "CZK",
            _ => null
        };
    }

    private static string? Blank(string? text)
    {
        var value = StatementValues.Collapse(text);

        return value.Length == 0 ? null : value;
    }

    [GeneratedRegex(@"UA\d{2}[0-9A-Z]{25}", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, 2000)]
    private static partial Regex IbanPattern();
}

namespace Deed.Application.BankConnections.Statements;

internal enum StatementField
{
    Date,
    Time,
    Description,
    Counterparty,
    Amount,
    Debit,
    Credit,
    Currency,
    Balance,
    Account,
    Kind
}

internal sealed record StatementColumnMap(int HeaderRow, IReadOnlyDictionary<StatementField, int> Columns)
{
    internal bool Has(StatementField field) => Columns.ContainsKey(field);

    internal string? Cell(string[] row, StatementField field)
        => Columns.TryGetValue(field, out var index) && index < row.Length ? row[index] : null;

    internal string Describe(IReadOnlyList<string[]> table)
    {
        var header = table[HeaderRow];

        return string.Join(", ", Columns.OrderBy(pair => pair.Value).Select(pair =>
            $"{pair.Key}={(pair.Value < header.Length ? header[pair.Value].Trim() : string.Empty)}"));
    }
}

internal static class StatementColumns
{
    private const int MaxHeaderScan = 40;

    private static readonly string[] BalanceMarkers = ["залишок", "баланс", "остаток", "balance"];
    private static readonly string[] DebitMarkers = ["дебет", "debit", "витрат", "списан", "withdraw", "расход"];
    private static readonly string[] CreditMarkers = ["кредит", "credit", "надходжен", "зарахуван", "поповнен", "deposit", "приход"];
    private static readonly string[] AmountMarkers = ["сума", "сумма", "amount", "sum"];
    private static readonly string[] CurrencyMarkers = ["валют", "currency", "ccy"];
    private static readonly string[] DateMarkers = ["дата", "date"];
    private static readonly string[] TimeMarkers = ["час", "time", "время"];
    private static readonly string[] CounterpartyMarkers = ["контрагент", "отримувач", "одержувач", "получател", "відправник", "counterparty", "merchant", "payee", "beneficiary"];
    private static readonly string[] DescriptionMarkers = ["призначення", "назначение", "опис", "описание", "деталі", "детали", "description", "details", "purpose", "narrative", "примітк", "коментар"];
    private static readonly string[] AccountMarkers = ["iban", "рахунок", "картк", "card", "account", "счет"];
    private static readonly string[] KindHeaders = ["тип", "тип операції", "type", "transaction type", "напрям", "direction", "d c", "dc"];
    private static readonly string[] PreferredMarkers = ["картк", "рахунк", "card", "account"];

    internal static StatementColumnMap? Detect(IReadOnlyList<string[]> table)
    {
        for (var rowIndex = 0; rowIndex < Math.Min(table.Count, MaxHeaderScan); rowIndex++)
        {
            var candidates = new Dictionary<StatementField, List<(int Column, string Header)>>();

            for (var column = 0; column < table[rowIndex].Length; column++)
            {
                var header = StatementValues.NormalizeHeader(table[rowIndex][column]);

                if (Classify(header) is { } field)
                {
                    if (!candidates.TryGetValue(field, out var list))
                    {
                        candidates[field] = list = [];
                    }

                    list.Add((column, header));
                }
            }

            if (candidates.ContainsKey(StatementField.Date)
                && (candidates.ContainsKey(StatementField.Amount) || candidates.ContainsKey(StatementField.Debit) || candidates.ContainsKey(StatementField.Credit)))
            {
                return new StatementColumnMap(rowIndex, candidates.ToDictionary(pair => pair.Key, pair => Pick(pair.Key, pair.Value)));
            }
        }

        return null;
    }

    private static int Pick(StatementField field, List<(int Column, string Header)> candidates)
    {
        if (field is StatementField.Amount or StatementField.Currency)
        {
            foreach (var candidate in candidates)
            {
                if (PreferredMarkers.Any(marker => candidate.Header.Contains(marker, StringComparison.OrdinalIgnoreCase)))
                {
                    return candidate.Column;
                }
            }
        }

        return candidates[0].Column;
    }

    private static StatementField? Classify(string header)
    {
        if (header.Length == 0)
        {
            return null;
        }

        if (KindHeaders.Contains(header, StringComparer.OrdinalIgnoreCase))
        {
            return StatementField.Kind;
        }

        if (header.Equals("дебет кредит", StringComparison.OrdinalIgnoreCase) || header.Equals("debit credit", StringComparison.OrdinalIgnoreCase))
        {
            return StatementField.Kind;
        }

        if (ContainsAny(header, BalanceMarkers))
        {
            return StatementField.Balance;
        }

        if (ContainsAny(header, DebitMarkers))
        {
            return StatementField.Debit;
        }

        if (ContainsAny(header, CreditMarkers))
        {
            return StatementField.Credit;
        }

        if (StartsWithAny(header, AmountMarkers))
        {
            return StatementField.Amount;
        }

        if (ContainsAny(header, CurrencyMarkers))
        {
            return StatementField.Currency;
        }

        if (StartsWithAny(header, TimeMarkers))
        {
            return StatementField.Time;
        }

        if (ContainsAny(header, DateMarkers))
        {
            return StatementField.Date;
        }

        if (ContainsAny(header, CounterpartyMarkers))
        {
            return StatementField.Counterparty;
        }

        if (ContainsAny(header, DescriptionMarkers))
        {
            return StatementField.Description;
        }

        return ContainsAny(header, AccountMarkers) ? StatementField.Account : null;
    }

    private static bool ContainsAny(string header, string[] markers)
        => markers.Any(marker => header.Contains(marker, StringComparison.OrdinalIgnoreCase));

    private static bool StartsWithAny(string header, string[] markers)
        => markers.Any(marker => header.StartsWith(marker, StringComparison.OrdinalIgnoreCase));
}

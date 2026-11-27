using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.BankConnections.Specifications;
using Deed.Application.BankNotifications.Responses;
using Deed.Application.BankNotifications.Specifications;
using Deed.Domain.Enums;
using Deed.Domain.Providers;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.BankNotifications.Queries.GetSummary;

internal sealed class GetBankTransactionsSummaryQueryHandler(
    IUser user,
    IBankNotificationRepository repository,
    IBankConnectionRepository connectionRepository,
    IDateTimeProvider dateTimeProvider)
    : IQueryHandler<GetBankTransactionsSummaryQuery, BankTransactionsSummaryResponse>
{
    private const int TopCount = 5;
    private const string OtherAccount = "Other";

    public async Task<Result<BankTransactionsSummaryResponse>> Handle(GetBankTransactionsSummaryQuery query, CancellationToken cancellationToken)
    {
        var period = Enum.TryParse<BankSummaryPeriod>(query.Period, true, out var parsed) ? parsed : BankSummaryPeriod.Month;
        var to = query.To ?? dateTimeProvider.UtcNow;
        var from = query.From ?? DefaultFrom(period, to);

        var connections = (await connectionRepository
            .GetAllAsync(new BankConnectionByUserSpecification(user.Id), cancellationToken)
            .ConfigureAwait(false)).ToList();

        var (connectionIds, includeUnassigned) = BankConnectionScope.Resolve(connections, query.Provider, query.ConnectionId);

        var specification = new BankTransactionsSummarySpecification(
            user.Id, query.ConnectionId, from, to, connectionIds, includeUnassigned);

        var totals = await repository.GetTotalsAsync(specification, period, cancellationToken).ConfigureAwait(false);
        var counterparties = await repository.GetCounterpartyTotalsAsync(specification, cancellationToken).ConfigureAwait(false);

        var labels = connections.ToDictionary(c => c.Id, c => c.Label);

        var currencies = totals
            .GroupBy(t => t.Currency ?? string.Empty)
            .Select(group => ToCurrency(group.Key, group.ToList(), counterparties, labels, period))
            .OrderByDescending(c => c.Income + c.Expense + c.ExchangeIn + c.ExchangeOut)
            .ToList();

        return Result.Success(new BankTransactionsSummaryResponse(period.ToString(), from, to, currencies));
    }

    private static BankSummaryCurrencyResponse ToCurrency(
        string currency,
        List<BankTransactionTotal> totals,
        IReadOnlyList<BankCounterpartyTotal> counterparties,
        Dictionary<int, string> labels,
        BankSummaryPeriod period)
    {
        var periods = totals
            .GroupBy(t => BucketStart(period, t))
            .Select(g => new BankSummaryPeriodResponse(g.Key, Income(g), Expense(g), g.Sum(t => t.Count)))
            .OrderByDescending(p => p.Start)
            .ToList();

        var types = totals
            .GroupBy(t => (t.Direction, t.Type))
            .Select(g => new BankSummaryTypeResponse(g.Key.Direction.ToString(), g.Key.Type.ToString(), g.Sum(t => t.Amount), g.Sum(t => t.Count)))
            .OrderByDescending(t => t.Amount)
            .ToList();

        var accounts = totals
            .GroupBy(t => t.BankConnectionId)
            .Select(g => new BankSummaryAccountResponse(
                g.Key,
                g.Key is { } id && labels.TryGetValue(id, out var label) ? label : OtherAccount,
                Income(g),
                Expense(g),
                g.Sum(t => t.Count)))
            .OrderByDescending(a => a.Income + a.Expense)
            .ToList();

        var own = counterparties.Where(c => (c.Currency ?? string.Empty) == currency).ToList();

        return new BankSummaryCurrencyResponse(
            currency,
            Income(totals),
            Expense(totals),
            Exchanged(totals, BankTransactionDirection.Incoming),
            Exchanged(totals, BankTransactionDirection.Outgoing),
            totals.Sum(t => t.Count),
            periods,
            types,
            accounts,
            Top(own, BankTransactionDirection.Incoming),
            Top(own, BankTransactionDirection.Outgoing));
    }

    private static List<BankSummaryCounterpartyResponse> Top(List<BankCounterpartyTotal> items, BankTransactionDirection direction)
        => items
            .Where(c => c.Direction == direction)
            .GroupBy(c => string.IsNullOrWhiteSpace(c.Counterparty) ? null : c.Counterparty.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(g => new BankSummaryCounterpartyResponse(g.Key, g.Sum(c => c.Amount), g.Sum(c => c.Count)))
            .OrderByDescending(c => c.Amount)
            .Take(TopCount)
            .ToList();

    private static decimal Income(IEnumerable<BankTransactionTotal> totals)
        => totals.Where(t => t.Direction == BankTransactionDirection.Incoming && t.Type != BankTransactionType.Exchange).Sum(t => t.Amount);

    private static decimal Expense(IEnumerable<BankTransactionTotal> totals)
        => totals.Where(t => t.Direction == BankTransactionDirection.Outgoing && t.Type != BankTransactionType.Exchange).Sum(t => t.Amount);

    private static decimal Exchanged(IEnumerable<BankTransactionTotal> totals, BankTransactionDirection direction)
        => totals.Where(t => t.Direction == direction && t.Type == BankTransactionType.Exchange).Sum(t => t.Amount);

    private static DateOnly BucketStart(BankSummaryPeriod period, BankTransactionTotal total)
        => period switch
        {
            BankSummaryPeriod.Day => new DateOnly(total.Year, total.Month, total.Day),
            BankSummaryPeriod.Month => new DateOnly(total.Year, total.Month, 1),
            _ => new DateOnly(total.Year, 1, 1)
        };

    private static DateTimeOffset DefaultFrom(BankSummaryPeriod period, DateTimeOffset to)
        => period switch
        {
            BankSummaryPeriod.Day => new DateTimeOffset(to.Year, to.Month, to.Day, 0, 0, 0, to.Offset).AddDays(-29),
            BankSummaryPeriod.Month => new DateTimeOffset(to.Year, to.Month, 1, 0, 0, 0, to.Offset).AddMonths(-11),
            _ => new DateTimeOffset(to.Year, 1, 1, 0, 0, 0, to.Offset).AddYears(-9)
        };
}

using Deed.Application.Abstractions.Data;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Deed.Infrastructure.Persistence.Repositories;

internal sealed class BankNotificationRepository(IDeedDbContext context)
    : GeneralRepository<BankNotification>(context), IBankNotificationRepository
{
    private const int MaxCounterparties = 500;

    public async Task<IReadOnlyList<BankTransactionTotal>> GetTotalsAsync(
        ISpecification<BankNotification> specification,
        BankSummaryPeriod period,
        CancellationToken cancellationToken = default)
    {
        var query = ApplySpecification(specification);

        return period switch
        {
            BankSummaryPeriod.Day => await query
                .GroupBy(n => new { n.BankConnectionId, n.Currency, n.Direction, n.Type, n.OccurredAt.Year, n.OccurredAt.Month, n.OccurredAt.Day })
                .Select(g => new BankTransactionTotal(
                    g.Key.BankConnectionId, g.Key.Currency, g.Key.Direction, g.Key.Type, g.Key.Year, g.Key.Month, g.Key.Day, g.Sum(n => n.Amount), g.Count()))
                .ToListAsync(cancellationToken),
            BankSummaryPeriod.Month => await query
                .GroupBy(n => new { n.BankConnectionId, n.Currency, n.Direction, n.Type, n.OccurredAt.Year, n.OccurredAt.Month })
                .Select(g => new BankTransactionTotal(
                    g.Key.BankConnectionId, g.Key.Currency, g.Key.Direction, g.Key.Type, g.Key.Year, g.Key.Month, 1, g.Sum(n => n.Amount), g.Count()))
                .ToListAsync(cancellationToken),
            _ => await query
                .GroupBy(n => new { n.BankConnectionId, n.Currency, n.Direction, n.Type, n.OccurredAt.Year })
                .Select(g => new BankTransactionTotal(
                    g.Key.BankConnectionId, g.Key.Currency, g.Key.Direction, g.Key.Type, g.Key.Year, 1, 1, g.Sum(n => n.Amount), g.Count()))
                .ToListAsync(cancellationToken)
        };
    }

    public async Task<IReadOnlyList<string>> GetExternalIdsAsync(ISpecification<BankNotification> specification, CancellationToken cancellationToken = default) =>
        await ApplySpecification(specification)
            .Select(n => n.ExternalId)
            .ToListAsync(cancellationToken);

    public async Task<int> MarkReadAsync(string createdBy, IReadOnlyCollection<int>? ids, CancellationToken cancellationToken = default)
    {
        var selected = ids?.ToList();

        return await DbSet
            .Where(n => n.CreatedBy == createdBy && !n.IsRead && (selected == null || selected.Contains(n.Id)))
            .ExecuteUpdateAsync(p => p.SetProperty(n => n.IsRead, true), cancellationToken);
    }

    public async Task<int> DeleteByConnectionsAsync(string createdBy, IReadOnlyCollection<int> connectionIds, CancellationToken cancellationToken = default)
    {
        var ids = connectionIds.ToList();

        return await DbSet
            .Where(n => n.CreatedBy == createdBy && n.BankConnectionId != null && ids.Contains(n.BankConnectionId.Value))
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<DateTimeOffset?> GetOldestAsync(ISpecification<BankNotification> specification, CancellationToken cancellationToken = default) =>
        await ApplySpecification(specification)
            .MinAsync(n => (DateTimeOffset?)n.OccurredAt, cancellationToken);

    public async Task<IReadOnlyList<BankCounterpartyTotal>> GetCounterpartyTotalsAsync(ISpecification<BankNotification> specification, CancellationToken cancellationToken = default) =>
        await ApplySpecification(specification)
            .GroupBy(n => new { n.Currency, n.Direction, n.Counterparty })
            .OrderByDescending(g => g.Sum(n => n.Amount))
            .Take(MaxCounterparties)
            .Select(g => new BankCounterpartyTotal(
                g.Key.Currency,
                g.Key.Direction,
                g.Key.Counterparty,
                g.Sum(n => n.Amount),
                g.Count()))
            .ToListAsync(cancellationToken);
}

using System.Diagnostics;
using System.Linq;
using Deed.Application.Exchanges.Service;
using Deed.Domain.Entities;
using Deed.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Quartz;
using Serilog;

namespace Deed.Infrastructure.BackgroundJobs.SaveLatestExchange;

[DisallowConcurrentExecution]
public sealed class UpsertLatestExchangeJob(
    IUnitOfWork unitOfWork,
    IExchangeRepository repository,
    IExchangeHttpService service)
    : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        Log.Information("Job started: {Name}", nameof(UpsertLatestExchangeJob));

        var latestExchangesResult = await service.GetCurrenciesAsync().ConfigureAwait(false);

        if (!latestExchangesResult.IsSuccess)
        {
            Log.Error("Exchange API request failed: {Message}", string.Join(", ", latestExchangesResult.Errors.Select(e => e.Message)));
            return;
        }

        var exchanges = await repository.GetAllAsync().ConfigureAwait(false);
        var lookup = exchanges
            .ToDictionary(
                e => key(e.NationalCurrencyCode, e.TargetCurrencyCode),
                e => e,
                StringComparer.OrdinalIgnoreCase);

        var entitiesToAdd = new HashSet<Exchange>();
        var entitiesToUpdate = new HashSet<Exchange>();

        foreach (var latestExchange in latestExchangesResult.Value)
        {
            if (!lookup.TryGetValue(key(latestExchange.NationalCurrencyCode, latestExchange.TargetCurrencyCode), out var existing))
            {
                entitiesToAdd.Add(latestExchange);
                continue;
            }

            if (existing.Buy.Equals(latestExchange.Buy) &&
                existing.Sale.Equals(latestExchange.Sale))
            {
                continue;
            }

            existing.Buy = latestExchange.Buy;
            existing.Sale = latestExchange.Sale;

            entitiesToUpdate.Add(existing);
        }

        if (entitiesToAdd.Count > 0)
        {
            repository.CreateRange(entitiesToAdd);
        }

        if (entitiesToUpdate.Count > 0)
        {
            repository.UpdateRange(entitiesToUpdate);
        }

        if (entitiesToAdd.Count > 0 || entitiesToUpdate.Count > 0)
        {
            await unitOfWork.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);
        }

        Log.Information("Job finished: {Name}", nameof(UpsertLatestExchangeJob));

        static string key(string x, string y) => $"{x}:{y}";
    }
}

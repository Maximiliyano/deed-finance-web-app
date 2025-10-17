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
    IExchangeRepository repository,
    IExchangeHttpService service)
    : IJob
{
    public async Task Execute(IJobExecutionContext context) // TODO tests
    {
        Log.Information("Job started: {Name}", nameof(UpsertLatestExchangeJob));

        var latestExchangesResult = await service.GetCurrenciesAsync();

        if (!latestExchangesResult.IsSuccess)
        {
            Log.Error("Exchange API request failed: {Message}", string.Join(", ", latestExchangesResult.Errors.Select(e => e.Message)));
            return;
        }

        Log.Information("Executed exchange from API successfully.");
        Log.Information("Adding / Updating current exchange...");

        var exchanges = await repository.GetAllAsync();
        var lookup = exchanges
            .AsParallel()
            .ToDictionary(
                e => key(e.NationalCurrencyCode, e.TargetCurrencyCode),
                e => e,
                StringComparer.OrdinalIgnoreCase);

        var entities = new HashSet<Exchange>();

        foreach (var latestExchange in latestExchangesResult.Value)
        {
            if (!lookup.TryGetValue(key(latestExchange.NationalCurrencyCode, latestExchange.TargetCurrencyCode), out var existing))
            {
                entities.Add(latestExchange);
                continue;
            }

            if (existing.Buy.Equals(latestExchange.Buy) &&
                existing.Sale.Equals(latestExchange.Sale))
            {
                continue;
            }

            existing.Buy = latestExchange.Buy;
            existing.Sale = latestExchange.Sale;

            entities.Add(existing);
        }

        await repository.UpsertAsync(entities, context.CancellationToken);

        Log.Information("Job finished: {Name}", nameof(UpsertLatestExchangeJob));

        static string key(string x, string y) => $"{x}:{y}";
    }
}

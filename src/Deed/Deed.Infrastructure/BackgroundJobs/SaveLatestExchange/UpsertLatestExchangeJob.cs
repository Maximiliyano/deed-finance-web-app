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
    IUnitOfWork unitOfWork,
    IExchangeHttpService service)
    : IJob
{
    public async Task Execute(IJobExecutionContext context) // TODO tests
    {
        Log.Information("Save latest exchange background job started.");

        var latestExchangesResult = await service.GetCurrenciesAsync();
        if (!latestExchangesResult.IsSuccess)
        {
            Log.Error("Exchange API request failed: {Message}", string.Join(", ", latestExchangesResult.Errors.Select(e => e.Message)));
            return;
        }

        Log.Information("Executed exchange from API successfully.");
        Log.Information("Adding / Updating current exchange...");

        var exchanges = await repository.GetAllAsync();
        var lookup = exchanges.ToDictionary(
            e => key(e.NationalCurrencyCode, e.TargetCurrencyCode),
            e => e,
            StringComparer.OrdinalIgnoreCase);

        var entitiesToUpdate = new List<Exchange>();
        var entitiesToCreate = new List<Exchange>();

        foreach (var latestExchange in latestExchangesResult.Value)
        {
            if (!lookup.TryGetValue(key(latestExchange.NationalCurrencyCode, latestExchange.TargetCurrencyCode), out var existing))
            {
                entitiesToCreate.Add(latestExchange);
                continue;
            }

            existing.Buy = latestExchange.Buy;
            existing.Sale = latestExchange.Sale;
            entitiesToUpdate.Add(existing);
        }

        if (entitiesToCreate.Any())
        {
            repository.AddRange(entitiesToCreate);
        }
        if (entitiesToUpdate.Any())
        {
            repository.UpdateRange(entitiesToUpdate);
        }

        if (entitiesToCreate.Count > 0 || entitiesToUpdate.Count > 0)
        {
            await unitOfWork.SaveChangesAsync(context.CancellationToken);
        }

        Log.Information("{Count} exchanges created. {Count2} exchanges updated.", entitiesToCreate.Count, entitiesToUpdate.Count);

        Log.Information("Save latest exchange background job finished.");
        
        static string key(string x, string y) => $"{x}:{y}";
    }
}

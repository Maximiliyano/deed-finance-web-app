using Deed.Application.Exchanges.Service;
using Deed.Domain.Entities;
using Deed.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Quartz;
using Serilog;

namespace Deed.Infrastructure.BackgroundJobs.SaveLatestExchange;

[DisallowConcurrentExecution]
public sealed class SaveLatestExchangeJob(
    IExchangeRepository repository,
    IUnitOfWork unitOfWork,
    IExchangeHttpService service)
    : IJob
{
    public async Task Execute(IJobExecutionContext context) // TODO tests
    {
        Log.Information("Save latest exchange background job has been started.");

        Log.Information("Executing exchange from API...");

        var latestExchangesResult = await service.GetCurrencyAsync();

        if (!latestExchangesResult.IsSuccess)
        {
            Log.Error("Error occured during executing exchange from API.");
            return;
        }

        Log.Information("Executed exchange from API successfully.");
        Log.Information("Adding / Updating current exchange...");

        var exchanges = (await repository.GetAllAsync()).ToList();
        var entitiesToUpdate = new HashSet<Exchange>();

        foreach (var latestExchange in latestExchangesResult.Value)
        {
            var exchange = exchanges.Find(x =>
                x.NationalCurrencyCode.Equals(latestExchange.NationalCurrencyCode, StringComparison.OrdinalIgnoreCase) &&
                x.TargetCurrencyCode.Equals(latestExchange.TargetCurrencyCode, StringComparison.OrdinalIgnoreCase));

            if (exchange is null)
            {
                continue;
            }

            exchange.Buy = latestExchange.Buy;
            exchange.Sale = latestExchange.Sale;

            entitiesToUpdate.Add(exchange);
        }

        repository.UpdateRange(entitiesToUpdate);

        await unitOfWork.SaveChangesAsync(context.CancellationToken);

        Log.Information("Current exchange has added.");
        Log.Information("Save latest exchange background job has been finished successfully.");
    }
}

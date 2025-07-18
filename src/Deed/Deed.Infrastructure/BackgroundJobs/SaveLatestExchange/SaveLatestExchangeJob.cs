using Deed.Application.Exchanges.Service;
using Deed.Domain.Entities;
using Deed.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Deed.Infrastructure.BackgroundJobs.SaveLatestExchange;

[DisallowConcurrentExecution]
public sealed class SaveLatestExchangeJob(
    IExchangeRepository repository,
    IUnitOfWork unitOfWork,
    IExchangeHttpService service,
    ILogger<SaveLatestExchangeJob> logger)
    : IJob
{
    public async Task Execute(IJobExecutionContext context) // TODO tests
    {
        logger.LogInformation("Save latest exchange background job has been started.");

        logger.LogInformation("Executing exchange from API...");

        var latestExchangesResult = await service.GetCurrencyAsync();

        if (!latestExchangesResult.IsSuccess)
        {
            logger.LogError("Error occured during executing exchange from API.");
            return;
        }

        logger.LogInformation("Executed exchange from API successfully.");
        logger.LogInformation("Adding / Updating current exchange...");

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

        logger.LogInformation("Current exchange has added.");
        logger.LogInformation("Save latest exchange background job has been finished successfully.");
    }
}

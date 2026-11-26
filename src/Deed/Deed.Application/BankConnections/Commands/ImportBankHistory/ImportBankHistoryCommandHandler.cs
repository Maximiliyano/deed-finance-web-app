using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.BankConnections.Responses;
using Deed.Application.BankConnections.Service;
using Deed.Domain.Errors;
using Deed.Domain.Providers;
using Deed.Domain.Results;

namespace Deed.Application.BankConnections.Commands.ImportBankHistory;

internal sealed class ImportBankHistoryCommandHandler(
    IUser user,
    IBankTransactionSyncService syncService,
    IDateTimeProvider dateTimeProvider,
    IBankBackfillTrigger backfillTrigger)
    : ICommandHandler<ImportBankHistoryCommand, BankHistoryImportResponse>
{
    public async Task<Result<BankHistoryImportResponse>> Handle(ImportBankHistoryCommand command, CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.UtcNow;
        var to = command.To is { } requested && requested < now ? requested : now;

        if (command.From >= to)
        {
            return Result.Failure<BankHistoryImportResponse>(DomainErrors.Bank.InvalidPeriod);
        }

        var response = await syncService.ImportHistoryAsync(user.Name!, command.From, to, cancellationToken).ConfigureAwait(false);

        if (response.InProgress)
        {
            await backfillTrigger.TriggerAsync(cancellationToken).ConfigureAwait(false);
        }

        return Result.Success(response);
    }
}

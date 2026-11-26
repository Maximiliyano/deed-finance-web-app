using Deed.Application.Abstractions.Messaging;
using Deed.Application.Abstractions.Services;
using Deed.Application.Auth;
using Deed.Application.BankConnections.Service;
using Deed.Application.BankConnections.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Providers;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using Serilog;

namespace Deed.Application.BankConnections.Commands.ConnectPrivat;

internal sealed class ConnectPrivatCommandHandler(
    IUser user,
    IBankConnectionRepository repository,
    IPrivatAutoclientService bank,
    IBankTransactionSyncService syncService,
    ITokenProtector tokenProtector,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ConnectPrivatCommand>
{
    private const string DefaultLabel = "Business";
    private const int HistoryDays = 30;

    public async Task<Result> Handle(ConnectPrivatCommand command, CancellationToken cancellationToken)
    {
        var iban = BankConnectionExtensions.NormalizeIban(command.Iban);
        var token = command.Token.Trim();
        var now = dateTimeProvider.UtcNow;

        var check = await bank
            .GetTransactionsAsync(token, iban, now.AddDays(-HistoryDays), now, cancellationToken)
            .ConfigureAwait(false);

        if (!check.IsSuccess)
        {
            return Result.Failure(check.Errors);
        }

        var existing = await repository
            .GetAsync(new BankConnectionByAccountSpecification(user.Name!, BankProvider.PrivatAutoclient, iban, tracking: true), cancellationToken)
            .ConfigureAwait(false);

        var encrypted = tokenProtector.Protect(token);
        var label = BankConnectionExtensions.NormalizeLabel(command.Label) ?? existing?.Label ?? DefaultLabel;

        BankConnection connection;

        if (existing is null)
        {
            connection = new BankConnection
            {
                Provider = BankProvider.PrivatAutoclient,
                EncryptedToken = encrypted,
                AccountIban = iban,
                Label = label,
                Status = BankConnectionStatus.Active,
                LastSyncAt = now
            };

            repository.Create(connection);
        }
        else
        {
            connection = existing;
            existing.EncryptedToken = encrypted;
            existing.Label = label;
            existing.Status = BankConnectionStatus.Active;
            existing.LastSyncAt = now;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var imported = await syncService
            .StageHistoryAsync(user.Name!, label, connection.Id, check.Value, cancellationToken)
            .ConfigureAwait(false);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Log.Information("Bank connection saved for {User}, imported {Imported} historical transactions", user.Name, imported);

        return Result.Success();
    }
}

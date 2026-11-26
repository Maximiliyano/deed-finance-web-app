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
    ITokenProtector tokenProtector,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ConnectPrivatCommand>
{
    public async Task<Result> Handle(ConnectPrivatCommand command, CancellationToken cancellationToken)
    {
        var iban = BankConnectionExtensions.NormalizeIban(command.Iban);
        var token = command.Token.Trim();
        var now = dateTimeProvider.UtcNow;

        var check = await bank
            .GetTransactionsAsync(token, iban, now.AddDays(-1), now, cancellationToken)
            .ConfigureAwait(false);

        if (!check.IsSuccess)
        {
            return Result.Failure(check.Errors);
        }

        var existing = await repository
            .GetAsync(new BankConnectionByUserSpecification(user.Name!, tracking: true), cancellationToken)
            .ConfigureAwait(false);

        var encrypted = tokenProtector.Protect(token);

        if (existing is null)
        {
            repository.Create(new BankConnection
            {
                Provider = BankProvider.PrivatAutoclient,
                EncryptedToken = encrypted,
                AccountIban = iban,
                Status = BankConnectionStatus.Active,
                LastSyncAt = now
            });
        }
        else
        {
            existing.Provider = BankProvider.PrivatAutoclient;
            existing.EncryptedToken = encrypted;
            existing.AccountIban = iban;
            existing.Status = BankConnectionStatus.Active;
            existing.LastSyncAt = now;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Log.Information("Bank connection saved for {User}", user.Name);

        return Result.Success();
    }
}

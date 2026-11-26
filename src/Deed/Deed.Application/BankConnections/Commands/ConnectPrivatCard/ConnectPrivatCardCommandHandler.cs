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

namespace Deed.Application.BankConnections.Commands.ConnectPrivatCard;

internal sealed class ConnectPrivatCardCommandHandler(
    IUser user,
    IBankConnectionRepository repository,
    IPrivatCardService bank,
    ITokenProtector tokenProtector,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ConnectPrivatCardCommand>
{
    public async Task<Result> Handle(ConnectPrivatCardCommand command, CancellationToken cancellationToken)
    {
        var card = BankConnectionExtensions.NormalizeCardNumber(command.CardNumber);
        var credentials = new PrivatCardCredentials(command.MerchantId.Trim(), command.Password);
        var now = dateTimeProvider.UtcNow;

        var check = await bank
            .GetTransactionsAsync(credentials, card, now.AddDays(-1), now, cancellationToken)
            .ConfigureAwait(false);

        if (!check.IsSuccess)
        {
            return Result.Failure(check.Errors);
        }

        var existing = await repository
            .GetAsync(new BankConnectionByUserSpecification(user.Name!, tracking: true), cancellationToken)
            .ConfigureAwait(false);

        var encrypted = tokenProtector.Protect(credentials.Serialize());

        if (existing is null)
        {
            repository.Create(new BankConnection
            {
                Provider = BankProvider.PrivatCard,
                EncryptedToken = encrypted,
                AccountIban = card,
                Status = BankConnectionStatus.Active,
                LastSyncAt = now
            });
        }
        else
        {
            existing.Provider = BankProvider.PrivatCard;
            existing.EncryptedToken = encrypted;
            existing.AccountIban = card;
            existing.Status = BankConnectionStatus.Active;
            existing.LastSyncAt = now;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Log.Information("Privat24 card connection saved for {User}", user.Name);

        return Result.Success();
    }
}

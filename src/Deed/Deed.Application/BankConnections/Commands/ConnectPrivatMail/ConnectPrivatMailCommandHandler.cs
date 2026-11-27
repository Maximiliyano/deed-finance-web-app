using Deed.Application.Abstractions.Messaging;
using Deed.Application.Abstractions.Settings;
using Deed.Application.Auth;
using Deed.Application.BankConnections.Responses;
using Deed.Application.BankConnections.Specifications;
using Deed.Application.BankConnections.Statements;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Providers;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using Microsoft.Extensions.Options;
using Serilog;

namespace Deed.Application.BankConnections.Commands.ConnectPrivatMail;

internal sealed class ConnectPrivatMailCommandHandler(
    IUser user,
    IBankConnectionRepository repository,
    IOptions<StatementMailSettings> options,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ConnectPrivatMailCommand, PrivatMailConnectionResponse>
{
    private const string LabelPrefix = "Privat24";

    public async Task<Result<PrivatMailConnectionResponse>> Handle(ConnectPrivatMailCommand command, CancellationToken cancellationToken)
    {
        if (!options.Value.IsEnabled)
        {
            return Result.Failure<PrivatMailConnectionResponse>(DomainErrors.Bank.MailNotConfigured);
        }

        var iban = BankConnectionExtensions.NormalizeIban(command.Iban);

        var existing = await repository
            .GetAsync(new BankConnectionByAccountSpecification(user.Name!, BankProvider.PrivatMail, iban), cancellationToken)
            .ConfigureAwait(false);

        if (existing is not null)
        {
            return Result.Failure<PrivatMailConnectionResponse>(DomainErrors.Bank.AccountAlreadyConnected);
        }

        var key = StatementMailAddress.NewKey();

        var connection = new BankConnection
        {
            Provider = BankProvider.PrivatMail,
            EncryptedToken = string.Empty,
            AccountIban = iban,
            ProviderAccountId = key,
            Label = BankConnectionExtensions.NormalizeLabel(command.Label) ?? $"{LabelPrefix} {iban[^4..]}",
            Currency = string.IsNullOrWhiteSpace(command.Currency) ? null : command.Currency.Trim().ToUpperInvariant(),
            Status = BankConnectionStatus.Active,
            LastSyncAt = dateTimeProvider.UtcNow
        };

        repository.Create(connection);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Log.Information("Statement mail connection {Id} created for {User}", connection.Id, user.Name);

        return Result.Success(new PrivatMailConnectionResponse(connection.Id, key));
    }
}

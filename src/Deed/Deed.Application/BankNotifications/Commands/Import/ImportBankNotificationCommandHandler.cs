using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.BankConnections.Specifications;
using Deed.Application.BankNotifications.Import;
using Deed.Application.BankNotifications.Specifications;
using Deed.Application.Categories.Specifications;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.BankNotifications.Commands.Import;

internal sealed class ImportBankNotificationCommandHandler(
    IUser user,
    IBankNotificationRepository notificationRepository,
    IBankConnectionRepository connectionRepository,
    ICategoryRepository categoryRepository,
    IBankTransactionImporter importer,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ImportBankNotificationCommand>
{
    public async Task<Result> Handle(ImportBankNotificationCommand command, CancellationToken cancellationToken)
    {
        var notification = await notificationRepository
            .GetAsync(new BankNotificationsByUserSpecification(user.Name!, ids: [command.Id], tracking: true), cancellationToken)
            .ConfigureAwait(false);

        if (notification is null)
        {
            return Result.Failure(DomainErrors.General.NotFound(nameof(notification)));
        }

        if (notification.ImportStatus != BankImportStatus.Pending)
        {
            return Result.Failure(DomainErrors.Bank.NotPending);
        }

        if (notification.BankConnectionId is not { } connectionId)
        {
            return Result.Failure(DomainErrors.Bank.CapitalNotLinked);
        }

        var connection = await connectionRepository
            .GetAsync(new BankConnectionByIdSpecification(connectionId, user.Name!), cancellationToken)
            .ConfigureAwait(false);

        if (connection is null || connection.IsExcluded)
        {
            return Result.Failure(DomainErrors.Bank.NotConnected);
        }

        if (command.CategoryId is { } categoryId)
        {
            var category = await categoryRepository
                .GetAsync(new CategoryByIdSpecification(categoryId), cancellationToken)
                .ConfigureAwait(false);

            if (category is null)
            {
                return Result.Failure(DomainErrors.General.NotFound(nameof(category)));
            }

            var expected = notification.Direction == BankTransactionDirection.Outgoing ? CategoryType.Expenses : CategoryType.Incomes;

            if (category.Type != expected)
            {
                return Result.Failure(DomainErrors.Bank.CategoryTypeMismatch);
            }
        }

        var imported = await importer.ImportAsync(connection, notification, command.CategoryId, cancellationToken).ConfigureAwait(false);

        if (!imported.IsSuccess)
        {
            return imported;
        }

        notification.IsRead = true;

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}

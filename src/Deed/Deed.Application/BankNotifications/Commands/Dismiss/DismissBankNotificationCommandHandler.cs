using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.BankNotifications.Specifications;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.BankNotifications.Commands.Dismiss;

internal sealed class DismissBankNotificationCommandHandler(
    IUser user,
    IBankNotificationRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DismissBankNotificationCommand>
{
    public async Task<Result> Handle(DismissBankNotificationCommand command, CancellationToken cancellationToken)
    {
        var notification = await repository
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

        notification.ImportStatus = BankImportStatus.Dismissed;
        notification.IsRead = true;

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}

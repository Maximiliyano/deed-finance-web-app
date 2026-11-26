using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.BankNotifications.Specifications;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.BankNotifications.Commands.MarkRead;

internal sealed class MarkBankNotificationsReadCommandHandler(
    IUser user,
    IBankNotificationRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<MarkBankNotificationsReadCommand>
{
    public async Task<Result> Handle(MarkBankNotificationsReadCommand command, CancellationToken cancellationToken)
    {
        var ids = command.Ids is { Count: > 0 } ? command.Ids : null;

        var unread = (await repository
            .GetAllAsync(new BankNotificationsByUserSpecification(user.Name!, unreadOnly: true, ids: ids, tracking: true), cancellationToken)
            .ConfigureAwait(false)).ToList();

        if (unread.Count == 0)
        {
            return Result.Success();
        }

        foreach (var notification in unread)
        {
            notification.IsRead = true;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}

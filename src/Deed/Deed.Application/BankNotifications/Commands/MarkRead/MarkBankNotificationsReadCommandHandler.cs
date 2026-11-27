using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.BankNotifications.Commands.MarkRead;

internal sealed class MarkBankNotificationsReadCommandHandler(
    IUser user,
    IBankNotificationRepository repository)
    : ICommandHandler<MarkBankNotificationsReadCommand>
{
    public async Task<Result> Handle(MarkBankNotificationsReadCommand command, CancellationToken cancellationToken)
    {
        var ids = command.Ids is { Count: > 0 } ? command.Ids : null;

        await repository.MarkReadAsync(user.Id, ids, cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}

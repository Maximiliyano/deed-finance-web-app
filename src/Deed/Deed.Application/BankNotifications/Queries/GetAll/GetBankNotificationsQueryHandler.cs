using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.BankNotifications.Responses;
using Deed.Application.BankNotifications.Specifications;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.BankNotifications.Queries.GetAll;

internal sealed class GetBankNotificationsQueryHandler(IUser user, IBankNotificationRepository repository)
    : IQueryHandler<GetBankNotificationsQuery, BankNotificationsResponse>
{
    private const int MaxLimit = 100;

    public async Task<Result<BankNotificationsResponse>> Handle(GetBankNotificationsQuery query, CancellationToken cancellationToken)
    {
        var limit = Math.Clamp(query.Limit, 1, MaxLimit);

        var items = await repository
            .GetAllAsync(new BankNotificationsByUserSpecification(user.Name!, query.UnreadOnly, take: limit), cancellationToken)
            .ConfigureAwait(false);

        var unread = await repository
            .CountAsync(new BankNotificationsByUserSpecification(user.Name!, unreadOnly: true), cancellationToken)
            .ConfigureAwait(false);

        return Result.Success(new BankNotificationsResponse(unread, items.ToResponses()));
    }
}

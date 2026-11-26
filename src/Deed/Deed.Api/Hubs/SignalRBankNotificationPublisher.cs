using Deed.Application.Abstractions.Services;
using Deed.Application.BankNotifications.Responses;
using Microsoft.AspNetCore.SignalR;

namespace Deed.Api.Hubs;

internal sealed class SignalRBankNotificationPublisher(IHubContext<BankNotificationsHub> hub)
    : IBankNotificationPublisher
{
    public Task PublishAsync(string userName, IReadOnlyList<BankNotificationResponse> notifications, CancellationToken cancellationToken)
        => hub.Clients.User(userName).SendAsync(BankNotificationsHub.ReceiveMethod, notifications, cancellationToken);
}

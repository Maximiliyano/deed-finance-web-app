using Deed.Application.BankNotifications.Responses;

namespace Deed.Application.Abstractions.Services;

public interface IBankNotificationPublisher
{
    Task PublishAsync(string userName, IReadOnlyList<BankNotificationResponse> notifications, CancellationToken cancellationToken);
}

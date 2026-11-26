using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Deed.Api.Hubs;

[Authorize]
internal sealed class BankNotificationsHub : Hub
{
    internal const string Route = "api/hubs/notifications";

    internal const string ReceiveMethod = "bankNotifications";
}

using Deed.Application.Auth;
using Microsoft.AspNetCore.SignalR;

namespace Deed.Api.Hubs;

internal sealed class NameUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
        => connection.User.FindFirst(AuthClaimTypes.Sub)?.Value;
}

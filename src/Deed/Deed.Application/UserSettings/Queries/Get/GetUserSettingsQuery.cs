using Deed.Application.Abstractions.Messaging;
using Deed.Application.UserSettings.Responses;

namespace Deed.Application.UserSettings.Queries.Get;

public sealed record GetUserSettingsQuery : IQuery<UserSettingsResponse?>;

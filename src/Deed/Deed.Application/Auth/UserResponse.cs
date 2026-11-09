using Deed.Application.UserSettings.Responses;

namespace Deed.Application.Auth;

public sealed record UserResponse(
    string? Id,
    string? Email,
    bool? EmailVerified,
    string? Fullname,
    IReadOnlyCollection<string> Roles,
    Uri? PictureUrl,
    UserSettingsResponse? Settings
);

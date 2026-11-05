using Deed.Application.UserSettings.Responses;

namespace Deed.Application.Auth;

public sealed record UserResponse(
    string? Id,
    string? Email,
    bool? EmailVerified,
    string? Fullname,
    Uri? PictureUrl,
    UserSettingsResponse? Settings
);

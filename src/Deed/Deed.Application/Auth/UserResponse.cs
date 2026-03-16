namespace Deed.Application.Auth;

public sealed record UserResponse(
    string? Email,
    bool? EmailVerified,
    string? Fullname,
    Uri? PictureUrl
);

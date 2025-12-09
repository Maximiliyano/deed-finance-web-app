using System.Security.Claims;

namespace Deed.Infrastructure.Persistence.Constants;

public static class AuthClaimTypes
{
    public const string Email = ClaimTypes.Email;
    public const string EmailVerified = "email_verified";

    public const string Name = "name";
    public const string Picture = "picture";
}

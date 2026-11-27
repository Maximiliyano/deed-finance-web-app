using Deed.Application.Abstractions.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Deed.Application.Auth;

public sealed class User(IHttpContextAccessor accessor, IOptions<AuthSettings> authSettings)
    : IUser
{
    public string Id => IsAuthenticated
        ? GetValue(AuthClaimTypes.Sub) ?? throw new InvalidOperationException("The authenticated user has no sub claim.")
        : AuthConstants.DefaultUserName;

    public string? Email => GetValue(AuthClaimTypes.Email);

    public bool? IsEmailVerified => bool.TryParse(GetValue(AuthClaimTypes.EmailVerified), out var v) && v;

    public string? Name => IsAuthenticated ? GetValue(AuthClaimTypes.Name) : null;

    public IReadOnlyCollection<string> Roles => GetValueList(authSettings.Value.RolesClaim);

    public Uri? PictureUrl
    {
        get
        {
            var p = GetValue(AuthClaimTypes.Picture);
            return p is not null
                ? new Uri(p)
                : null;
        }
    }

    public bool IsAuthenticated => accessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    private string? GetValue(string claimType)
    {
        return accessor.HttpContext?.User?.FindFirst(c => c.Type == claimType)?.Value;
    }

    private List<string> GetValueList(string claimType)
    {
        return accessor.HttpContext?.User?.FindAll(claimType).Select(c => c.Value)?.ToList();
    }
}

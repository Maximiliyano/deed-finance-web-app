using Deed.Application.Abstractions.Settings;
using Deed.Application.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Deed.Api.Endpoints.User;

internal sealed class Login : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/auth/login", (IOptions<WebUrlSettings> authSettings, string? returnUrl) => Results.Challenge(new AuthenticationProperties
        {
            RedirectUri = string.IsNullOrWhiteSpace(returnUrl)
                ? authSettings.Value.UIUrl
                : $"{authSettings.Value.UIUrl}{returnUrl}",
            Items = { { AuthConstants.ExplicitLoginKey, "true" } }
        }, [AuthConstants.AuthenticationScheme]))
        .WithTags(nameof(User))
        .AllowAnonymous();
    }
}

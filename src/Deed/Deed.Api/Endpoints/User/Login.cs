using Deed.Application.Abstractions.Settings;
using Deed.Application.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Deed.Api.Endpoints.User;

internal sealed class Login : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/auth/login", (IOptions<WebUrlSettings> webUrlSettings, string? returnUrl) =>
        {
            string baseUrl = webUrlSettings.Value.UIUrl;

            string redirectUri = !string.IsNullOrWhiteSpace(returnUrl) && returnUrl.StartsWith('/')
                ? $"{baseUrl}{returnUrl}"
                : baseUrl;

            return Results.Challenge(new AuthenticationProperties
            {
                RedirectUri = redirectUri,
                Items = { { AuthConstants.ExplicitLoginKey, "true" } }
            }, [AuthConstants.AuthenticationScheme]);
        })
        .WithTags(nameof(User))
        .AllowAnonymous();
    }
}

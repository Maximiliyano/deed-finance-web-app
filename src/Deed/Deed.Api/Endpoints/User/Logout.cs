using Deed.Application.Abstractions.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

namespace Deed.Api.Endpoints.User;

internal sealed class Logout : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/auth/logout", async (HttpContext context, IOptions<WebUrlSettings> webUrlSettings, IOptions<AuthSettings> authSettings) =>
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            var logoutUri = $"{authSettings.Value.Domain.TrimEnd('/')}/v2/logout?client_id={authSettings.Value.ClientID}&returnTo={Uri.EscapeDataString(webUrlSettings.Value.UIUrl)}";
            context.Response.Redirect(logoutUri);
        })
        .RequireAuthorization()
        .WithTags(nameof(User));
    }
}
